// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.IO;
using System.Linq;
using System.Text;
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    public class FileUploadValidatorTests
    {
        private static byte[] Bytes(params int[] b) => b.Select(x => (byte)x).ToArray();

        private static readonly byte[] PngHeader = Bytes(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D);
        private static readonly byte[] JpegHeader = Bytes(0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46);
        private static readonly byte[] WebpHeader = Bytes(0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50);
        private static readonly byte[] Mp4Header = Bytes(0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32);
        private static readonly byte[] WebmHeader = Bytes(0x1A, 0x45, 0xDF, 0xA3, 0x01, 0x00, 0x00, 0x00);
        private static readonly byte[] ExeHeader = Bytes(0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00); // MZ
        private static readonly byte[] SvgBytes = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>");

        private static UploadValidationResult Image(byte[] data, string name, long max = FileUploadValidator.DefaultMaxImageBytes)
            => FileUploadValidator.ValidateImage(new MemoryStream(data), data.Length, name, max);

        private static UploadValidationResult Video(byte[] data, string name, long max = FileUploadValidator.DefaultMaxVideoBytes)
            => FileUploadValidator.ValidateVideo(new MemoryStream(data), data.Length, name, max);

        [Fact]
        public void ValidatePng_RealPng_Ok()
        {
            var r = Image(PngHeader, "logo.png");
            r.IsValid.Should().BeTrue(r.Reason);
            r.DetectedType.Should().Be("png");
        }

        [Fact]
        public void ValidateJpeg_RealJpeg_Ok()
        {
            Image(JpegHeader, "shot.jpg").IsValid.Should().BeTrue();
            Image(JpegHeader, "shot.jpeg").IsValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateWebp_RealWebp_Ok()
        {
            Image(WebpHeader, "anim.webp").IsValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateImage_SvgRenamedToPng_Rejected()
        {
            var r = Image(SvgBytes, "evil.png");
            r.IsValid.Should().BeFalse("an SVG has no binary image magic and is script-bearing");
            r.Reason.Should().Contain("sniff");
        }

        [Fact]
        public void ValidateImage_ExeRenamedToPng_Rejected()
        {
            Image(ExeHeader, "malware.png").IsValid.Should().BeFalse("an EXE is not an image");
        }

        [Fact]
        public void ValidateImage_RealPngButWrongExtension_Rejected()
        {
            var r = Image(PngHeader, "logo.jpg");
            r.IsValid.Should().BeFalse("the extension must match the sniffed type");
            r.Reason.Should().Contain("does not match");
        }

        [Fact]
        public void ValidateImage_OverSizeLimit_Rejected()
        {
            var r = Image(PngHeader, "logo.png", max: PngHeader.Length - 1);
            r.IsValid.Should().BeFalse();
            r.Reason.Should().Contain("exceeds");
        }

        [Fact]
        public void ValidateImage_Empty_Rejected()
        {
            FileUploadValidator.ValidateImage(new MemoryStream(new byte[0]), 0, "x.png").IsValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateVideo_RealMp4_Ok()
        {
            var r = Video(Mp4Header, "clip.mp4");
            r.IsValid.Should().BeTrue(r.Reason);
            r.DetectedType.Should().Be("mp4");
        }

        [Fact]
        public void ValidateVideo_RealWebm_Ok()
        {
            Video(WebmHeader, "clip.webm").IsValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateVideo_ImageContentInVideoSlot_Rejected()
        {
            Video(PngHeader, "clip.mp4").IsValid.Should().BeFalse("a PNG is not a video");
        }
    }
}
