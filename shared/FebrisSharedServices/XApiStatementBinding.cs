// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.ViewModels.XApi;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    /// <summary>
    /// Helper for the xAPI statement ingest path that reads the incoming
    /// POST body ONCE, captures the verbatim bytes, parses them as a
    /// typed <see cref="XApiStatementDto"/>, and bundles both into an
    /// <see cref="XApiStatementSubmission"/>.
    /// <para>
    /// Replaces the previous <c>[FromBody] JObject</c> pattern with the
    /// audit-grade raw-byte preservation strategy.
    /// Storing the producer's exact bytes alongside the typed Statement
    /// gives F500 InfoSec auditors a definitive "what was sent" record;
    /// the typed DTO gives the BLL a strongly-typed surface to map from.
    /// </para>
    /// <para>
    /// <b>Why it lives in SharedServices:</b> it is a stateless request
    /// binder plus a file writer with no data access and no business
    /// logic, and SharedServices is the only shared library an edge
    /// deployment (enduser/pc/mobile) is allowed to reference. It was
    /// relocated here from SharedLogicLayer so the EndUser API can use it
    /// without crossing the core/edge trust boundary. SharedServices already
    /// references Microsoft.AspNetCore.Http, so this adds no new dependency.
    /// </para>
    /// <para>
    /// <b>Why a static helper instead of an <c>IModelBinder</c>:</b>
    /// <list type="bullet">
    ///   <item>No hard dependency on <c>Microsoft.AspNetCore.Mvc.Abstractions</c>
    ///     -- adding it for one binder isn't worth the surface-area
    ///     expansion.</item>
    ///   <item>Controllers call it as a one-liner:
    ///     <c>var sub = await XApiStatementBinding.ReadAsync(Request);</c></item>
    ///   <item>No DI plumbing, no <c>IModelBinderProvider</c> registration,
    ///     no per-host duplication of binder wiring.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Reading the body once vs <c>EnableBuffering()</c>:</b> we read
    /// the body fully into memory and parse off the in-memory copy. No
    /// stream re-positioning, so callers don't have to enable buffering.
    /// The trade-off is a peak-memory burst equal to the body size --
    /// fine for xAPI statements (typically 1-10 KiB, capped at 1 MiB
    /// below).
    /// </para>
    /// </summary>
    public static class XApiStatementBinding
    {
        /// <summary>
        /// Default cap on the request body size. 1 MiB matches reasonable
        /// xAPI statement size budgets (a single statement is typically
        /// 1-10 KiB; this ceiling tolerates vendor-extension-heavy
        /// payloads while protecting against runaway uploads).
        /// </summary>
        public const int DefaultMaxBodyBytes = 1 * 1024 * 1024;

        /// <summary>
        /// Filename suffix for the verbatim-bytes copy, written as
        /// <c>{statementUuid}.raw.json</c>.
        /// <para>
        /// T4. This used to be plain <c>.json</c>, which is byte-for-byte the name
        /// <c>StatementLogic.SavingJSONStatement</c> writes through
        /// <c>IStatementFileHandler.UploadPackage</c>, in the same directory, also truncating. Both
        /// ran for the same statement on <c>/Submit</c> and the raw write landed second, so the
        /// normalized copy was destroyed on every accepted statement and the node retained one
        /// representation where the code reads as though it retains two.
        /// </para>
        /// <para>
        /// The two artifacts are NOT interchangeable, which is why both are kept.
        /// <c>SavingJSONStatement</c> writes a re-serialized, lowercased, null-stripped,
        /// backslash-stripped rendering of the FACTORED statement. Only these raw bytes are what
        /// the producer actually sent, so only these can settle a dispute about what arrived.
        /// </para>
        /// </summary>
        public const string RawBodyFileSuffix = ".raw.json";

        /// <summary>
        /// Read + capture + parse the request body. Returns a populated
        /// <see cref="XApiStatementSubmission"/>; controllers branch on
        /// <see cref="XApiStatementSubmission.DtoBound"/> to decide
        /// whether to 400 the request or proceed.
        /// </summary>
        public static async Task<XApiStatementSubmission> ReadAsync(
            HttpRequest request,
            int maxBodyBytes = DefaultMaxBodyBytes)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // ---- Step 1: capture raw bytes verbatim ----
            byte[] rawBytes;
            using (var ms = new MemoryStream())
            {
                // Cap at maxBodyBytes + 1 so we can detect overrun without
                // tying up memory on a hostile multi-GB upload.
                var capped = new LimitedStream(request.Body, maxBodyBytes + 1);
                await capped.CopyToAsync(ms);
                if (ms.Length > maxBodyBytes)
                {
                    return new XApiStatementSubmission
                    {
                        Dto = null,
                        RawBody = null,
                        RawBodyEncoding = null,
                        ContentType = request.ContentType,
                        ParseError = $"Request body exceeded {maxBodyBytes} bytes.",
                    };
                }
                rawBytes = ms.ToArray();
            }

            // ---- Step 2: resolve charset ----
            // Honor an explicit charset= parameter on Content-Type; default
            // to UTF-8 per RFC 8259 (the JSON spec). Some xAPI producers
            // emit application/json with no charset; that's fine -- UTF-8
            // is the correct default.
            string charset = "utf-8";
            string contentType = request.ContentType;
            if (!string.IsNullOrEmpty(contentType))
            {
                try
                {
                    var ct = new ContentType(contentType);
                    if (!string.IsNullOrEmpty(ct.CharSet))
                    {
                        charset = ct.CharSet;
                    }
                }
                catch (System.Exception ex)
                {
                    Febris.SharedServices.FebrisLog.Error(ex, "XApiStatementBinding.ReadAsync: suppressed exception");
                    // Malformed Content-Type -- fall back to default.
                }
            }

            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(charset);
            }
            catch (System.Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "XApiStatementBinding.ReadAsync: suppressed exception");
                encoding = Encoding.UTF8;
            }

            // ---- Step 3: decode + parse ----
            XApiStatementDto dto = null;
            string parseError = null;
            try
            {
                string json = encoding.GetString(rawBytes);
                if (string.IsNullOrWhiteSpace(json))
                {
                    parseError = "Empty request body.";
                }
                else
                {
                    dto = JsonConvert.DeserializeObject<XApiStatementDto>(json);
                    if (dto == null)
                    {
                        // JSON parsed but deserialized to null (e.g. body
                        // was literal "null"). Treat as a parse failure
                        // so the controller can 400.
                        parseError = "Statement deserialized to null.";
                    }
                }
            }
            catch (JsonException ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "XApiStatementBinding.ReadAsync: suppressed exception");
                parseError = "Invalid JSON: " + ex.Message;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex, "XApiStatementBinding.ReadAsync: suppressed exception");
                parseError = "Decode/parse error: " + ex.Message;
            }

            return new XApiStatementSubmission
            {
                Dto = dto,
                RawBody = rawBytes,
                RawBodyEncoding = encoding.WebName,
                ContentType = contentType,
                ParseError = parseError,
            };
        }

        /// <summary>
        /// Persist the raw POST body bytes that <see cref="ReadAsync"/>
        /// captured, keyed on the persisted statement's UUID. Mirrors
        /// the existing <c>StaticDetails.JSONStatementFileSystemPath</c>
        /// + <c>{uuid}.json</c> naming so the audit-trail directory
        /// stays uniform.
        /// <para>
        /// Why a static helper next to <see cref="ReadAsync"/>: keeps
        /// the "capture + persist" lifecycle of the raw bytes in one
        /// module. Controllers don't need their own I/O dependency
        /// injection -- one-line call: <c>await XApiStatementBinding.PersistRawBytesAsync(submission.RawBody, statement.UUID);</c>
        /// </para>
        /// <para>
        /// Failure mode: write failures are logged to stdout (matches
        /// the rest of this codebase's catch-and-Console.WriteLine
        /// pattern) and the method returns false. The caller decides
        /// whether to escalate -- typically a missing audit file isn't
        /// a request-failure condition (the statement is in the DB),
        /// it's a monitoring concern.
        /// </para>
        /// </summary>
        /// <param name="rawBytes">Verbatim body bytes from
        /// <see cref="XApiStatementSubmission.RawBody"/>.</param>
        /// <param name="statementUuid">Persisted statement UUID, used
        /// as the filename stem. Required.</param>
        /// <param name="overrideDirectory">Optional directory override.
        /// Defaults to <c>StaticDetails.JSONStatementFileSystemPath</c>,
        /// which the legacy <c>SavingJSONStatement</c> path ALSO writes
        /// to. See <see cref="RawBodyFileSuffix"/> for why that matters.</param>
        public static async Task<bool> PersistRawBytesAsync(
            byte[] rawBytes,
            Guid statementUuid,
            string overrideDirectory = null)
        {
            if (rawBytes == null || rawBytes.Length == 0) return false;
            if (statementUuid == Guid.Empty) return false;

            string dir = string.IsNullOrEmpty(overrideDirectory)
                ? StaticDetails.JSONStatementFileSystemPath
                : overrideDirectory;

            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string fullPath = Path.Combine(dir, statementUuid.ToString() + RawBodyFileSuffix);
                // Use Create + WriteAsync for clean overwrite semantics.
                // If a previous attempt for the same UUID partially wrote
                // (e.g. process kill mid-flush), the next attempt should
                // be authoritative. xAPI statement UUIDs are unique per
                // submission, so overwrites of a different statement
                // shouldn't happen in practice.
                //
                // T4: that reasoning is about two different STATEMENTS colliding, which is indeed
                // impossible. It never considered a second WRITER for the same statement, which is
                // what was actually happening. This method used to write "{uuid}.json", and so does
                // StatementLogic.SavingJSONStatement via IStatementFileHandler.UploadPackage --
                // same directory, same name, both truncating. On /Submit both ran for one
                // statement, the normalized JSON first from the BLL and then these raw bytes from
                // the controller, so the raw write silently destroyed the normalized one and the
                // node kept exactly ONE of the two representations it believed it was keeping.
                // The suffix below separates them. See RawBodyFileSuffix.
                using (FileStream stream = File.Create(fullPath))
                {
                    await stream.WriteAsync(rawBytes, 0, rawBytes.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                // Match the codebase pattern: log + swallow. Caller can
                // check return value if they want stricter behavior.
                Febris.SharedServices.FebrisLog.Info(
                    $"PersistRawBytesAsync failed for statement {statementUuid}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stream wrapper that throws after <see cref="maxBytes"/> have been
        /// read. Used to bail out of pathologically-large uploads before
        /// fully buffering them. Read-only forward; mirrors the subset of
        /// Stream surface we need for CopyToAsync.
        /// </summary>
        private sealed class LimitedStream : Stream
        {
            private readonly Stream _inner;
            private readonly long _maxBytes;
            private long _read;

            public LimitedStream(Stream inner, long maxBytes)
            {
                _inner = inner;
                _maxBytes = maxBytes;
            }

            // Cap the inner read at the remaining budget so we never
            // pull more bytes than we'll surface. The original "let
            // inner read freely then return 0" approach lost bytes from
            // the inner stream (consumed but unsurfaced), which broke
            // the overflow-detection check that depends on the output
            // MemoryStream's Length exceeding maxBodyBytes.
            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_read >= _maxBytes) return 0;
                int allowed = (int)Math.Min(count, _maxBytes - _read);
                int n = _inner.Read(buffer, offset, allowed);
                _read += n;
                return n;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken ct)
            {
                if (_read >= _maxBytes) return 0;
                int allowed = (int)Math.Min(count, _maxBytes - _read);
                int n = await _inner.ReadAsync(buffer, offset, allowed, ct);
                _read += n;
                return n;
            }

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => _read; set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
