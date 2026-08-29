// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

namespace Febris.SharedServices
{
    /// <summary>
    /// Outcome of a <see cref="FileUploadValidator"/> check. <see cref="IsValid"/> is false
    /// when the upload should be rejected, with a human-readable <see cref="Reason"/>.
    /// </summary>
    public sealed class UploadValidationResult
    {
        public bool IsValid { get; }
        public string Reason { get; }
        public string DetectedType { get; }

        private UploadValidationResult(bool ok, string reason, string detected)
        {
            IsValid = ok;
            Reason = reason;
            DetectedType = detected;
        }

        public static UploadValidationResult Ok(string detectedType) => new UploadValidationResult(true, null, detectedType);
        public static UploadValidationResult Fail(string reason) => new UploadValidationResult(false, reason, null);
    }

    /// <summary>
    /// A magic-byte signature. A null entry in <see cref="Pattern"/> is a wildcard byte,
    /// used for container formats whose header has variable bytes (RIFF size, ftyp box).
    /// </summary>
    internal sealed class FileSignature
    {
        public string Name { get; set; }
        public int Offset { get; set; }
        public byte?[] Pattern { get; set; }
        public string[] Extensions { get; set; }
    }

    /// <summary>
    /// Validates user uploads by sniffing the real content type from the leading bytes
    /// (not the client-supplied Content-Type or file extension), enforcing a hard size cap,
    /// and requiring the extension to be consistent with the sniffed type. Script-bearing or
    /// spoofed types (for example an SVG or an EXE renamed to .png) fail the sniff and are
    /// rejected. This is the shared core every upload surface validates through.
    /// </summary>
    public static class FileUploadValidator
    {
        public const long DefaultMaxImageBytes = 15L * 1024 * 1024;        // 15 MB
        public const long DefaultMaxVideoBytes = 2L * 1024 * 1024 * 1024;  // 2 GB

        private static byte?[] Sig(params int[] bytes) => bytes.Select(b => (byte?)(byte)b).ToArray();
        // -1 marks a wildcard byte.
        private static byte?[] SigW(params int[] bytes) => bytes.Select(b => b < 0 ? (byte?)null : (byte?)(byte)b).ToArray();

        private static readonly FileSignature[] ImageSignatures =
        {
            new FileSignature { Name = "jpeg", Offset = 0, Pattern = Sig(0xFF, 0xD8, 0xFF), Extensions = new[] { ".jpg", ".jpeg" } },
            new FileSignature { Name = "png",  Offset = 0, Pattern = Sig(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A), Extensions = new[] { ".png" } },
            new FileSignature { Name = "gif",  Offset = 0, Pattern = Sig(0x47, 0x49, 0x46, 0x38), Extensions = new[] { ".gif" } },
            new FileSignature { Name = "webp", Offset = 0, Pattern = SigW(0x52, 0x49, 0x46, 0x46, -1, -1, -1, -1, 0x57, 0x45, 0x42, 0x50), Extensions = new[] { ".webp" } },
            new FileSignature { Name = "bmp",  Offset = 0, Pattern = Sig(0x42, 0x4D), Extensions = new[] { ".bmp" } },
        };

        private static readonly FileSignature[] VideoSignatures =
        {
            // ISO base media (mp4 / m4v / mov): 'ftyp' box at offset 4.
            new FileSignature { Name = "mp4",  Offset = 4, Pattern = Sig(0x66, 0x74, 0x79, 0x70), Extensions = new[] { ".mp4", ".m4v", ".mov" } },
            new FileSignature { Name = "webm", Offset = 0, Pattern = Sig(0x1A, 0x45, 0xDF, 0xA3), Extensions = new[] { ".webm", ".mkv" } },
            new FileSignature { Name = "avi",  Offset = 0, Pattern = SigW(0x52, 0x49, 0x46, 0x46, -1, -1, -1, -1, 0x41, 0x56, 0x49, 0x20), Extensions = new[] { ".avi" } },
            new FileSignature { Name = "mpeg", Offset = 0, Pattern = Sig(0x00, 0x00, 0x01, 0xBA), Extensions = new[] { ".mpg", ".mpeg" } },
        };

        // ---- IFormFile entry points ----

        public static UploadValidationResult ValidateImage(IFormFile file, long maxBytes = DefaultMaxImageBytes)
            => ValidateFormFile(file, maxBytes, ImageSignatures, "image");

        public static UploadValidationResult ValidateVideo(IFormFile file, long maxBytes = DefaultMaxVideoBytes)
            => ValidateFormFile(file, maxBytes, VideoSignatures, "video");

        private static UploadValidationResult ValidateFormFile(IFormFile file, long maxBytes, FileSignature[] allowed, string kind)
        {
            if (file == null || file.Length == 0) return UploadValidationResult.Fail("No file, or the file is empty.");
            using (var stream = file.OpenReadStream())
            {
                return Validate(stream, file.Length, file.FileName, maxBytes, allowed, kind);
            }
        }

        // ---- Stream entry points (unit-testable without ASP.NET) ----

        public static UploadValidationResult ValidateImage(Stream content, long length, string fileName, long maxBytes = DefaultMaxImageBytes)
            => Validate(content, length, fileName, maxBytes, ImageSignatures, "image");

        public static UploadValidationResult ValidateVideo(Stream content, long length, string fileName, long maxBytes = DefaultMaxVideoBytes)
            => Validate(content, length, fileName, maxBytes, VideoSignatures, "video");

        private static UploadValidationResult Validate(Stream content, long length, string fileName, long maxBytes, FileSignature[] allowed, string kind)
        {
            if (content == null || length <= 0) return UploadValidationResult.Fail("No content.");
            if (length > maxBytes)
            {
                return UploadValidationResult.Fail($"File size {length} bytes exceeds the {kind} limit of {maxBytes} bytes.");
            }

            string ext = (Path.GetExtension(fileName ?? string.Empty) ?? string.Empty).ToLowerInvariant();

            int needed = allowed.Max(s => s.Offset + s.Pattern.Length);
            byte[] header = ReadHeader(content, needed);

            FileSignature match = allowed.FirstOrDefault(sig => Matches(header, sig));
            if (match == null)
            {
                return UploadValidationResult.Fail(
                    $"File content is not a recognized {kind} type (magic-byte sniff failed). SVG and other script-bearing or spoofed types are rejected.");
            }

            // Reject double-extension / spoof tricks: the extension must match the sniffed type.
            if (!string.IsNullOrEmpty(ext) && !match.Extensions.Contains(ext))
            {
                return UploadValidationResult.Fail(
                    $"File extension '{ext}' does not match the detected {kind} content type '{match.Name}'.");
            }

            return UploadValidationResult.Ok(match.Name);
        }

        private static byte[] ReadHeader(Stream s, int count)
        {
            if (s.CanSeek) s.Seek(0, SeekOrigin.Begin);
            byte[] buf = new byte[count];
            int total = 0, read;
            while (total < count && (read = s.Read(buf, total, count - total)) > 0) total += read;
            if (total < count) Array.Resize(ref buf, total);
            return buf;
        }

        private static bool Matches(byte[] header, FileSignature sig)
        {
            if (header.Length < sig.Offset + sig.Pattern.Length) return false;
            for (int i = 0; i < sig.Pattern.Length; i++)
            {
                byte? expected = sig.Pattern[i];
                if (expected.HasValue && header[sig.Offset + i] != expected.Value) return false;
            }
            return true;
        }
    }
}
