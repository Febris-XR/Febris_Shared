// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.ViewModels.XApi;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for the xAPI statement ingest binding helper (Phase 3 of the
    /// statement-ingest optimization work).
    /// <para>
    /// Covers:
    /// <list type="bullet">
    ///   <item>DTO deserialization happy path against a representative
    ///     xAPI 1.0.3 statement payload.</item>
    ///   <item>Vendor-extension preservation via <c>[JsonExtensionData]</c>
    ///     (Option A's defense-in-depth alongside the raw-bytes capture).</item>
    ///   <item><c>XApiStatementBinding.ReadAsync</c> bundles parsed DTO
    ///     + verbatim bytes + content-type metadata; raw bytes are
    ///     byte-for-byte equal to the input stream.</item>
    ///   <item>Malformed JSON gracefully reports through
    ///     <c>ParseError</c> without throwing; bytes still captured
    ///     for "what did the client send" diagnostics.</item>
    ///   <item><c>PersistRawBytesAsync</c> writes verbatim bytes
    ///     (no encoding round-trip) and round-trips byte-for-byte.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class XApiStatementBindingTests
    {
        // A realistic xAPI 1.0.3 "completed" statement covering Actor with
        // mbox + name, Verb with display, Object (Activity) with definition,
        // Result with score and duration, Context with registration +
        // platform, plus a vendor-extension we expect to round-trip.
        private const string SampleStatementJson = @"{
            ""id"": ""b3e5cca0-b6f3-4b9a-b29c-5fa3a3da4b71"",
            ""actor"": {
                ""objectType"": ""Agent"",
                ""name"": ""Alice Trainee"",
                ""mbox"": ""mailto:alice@example.com""
            },
            ""verb"": {
                ""id"": ""http://adlnet.gov/expapi/verbs/completed"",
                ""display"": { ""en-US"": ""completed"" }
            },
            ""object"": {
                ""objectType"": ""Activity"",
                ""id"": ""https://febr.is/curricula/safety-101/module-3"",
                ""definition"": {
                    ""name"": { ""en-US"": ""Module 3 -- Lockout/Tagout"" },
                    ""description"": { ""en-US"": ""Hands-on LOTO simulation."" },
                    ""type"": ""http://adlnet.gov/expapi/activities/lesson""
                }
            },
            ""result"": {
                ""score"": { ""scaled"": 0.92, ""raw"": 92, ""min"": 0, ""max"": 100 },
                ""success"": true,
                ""completion"": true,
                ""duration"": ""PT4M30S""
            },
            ""context"": {
                ""registration"": ""15dd2c52-c5b3-4e1c-99f1-2c4e7a3a85b3"",
                ""platform"": ""Febris PC Launcher V3"",
                ""language"": ""en-US""
            },
            ""timestamp"": ""2026-05-22T18:30:00Z"",
            ""version"": ""1.0.3"",
            ""febris:trainingStationId"": ""station-42"",
            ""febris:hardwareSerial"": ""XR-9001-A""
        }";

        // ---------- DTO deserialization ------------------------------

        [Fact]
        public void XApiStatementDto_HappyPath_DeserializesAllKeyFields()
        {
            var dto = JsonConvert.DeserializeObject<XApiStatementDto>(SampleStatementJson);

            dto.Should().NotBeNull();
            dto.Id.Should().Be("b3e5cca0-b6f3-4b9a-b29c-5fa3a3da4b71");
            dto.Actor.Mbox.Should().Be("mailto:alice@example.com");
            dto.Actor.Name.Should().Be("Alice Trainee");
            dto.Verb.Id.Should().Be("http://adlnet.gov/expapi/verbs/completed");
            dto.Verb.Display.Should().ContainKey("en-US").WhoseValue.Should().Be("completed");
            dto.Object.Id.Should().Be("https://febr.is/curricula/safety-101/module-3");
            dto.Object.Definition.Type.Should().Be("http://adlnet.gov/expapi/activities/lesson");
            dto.Result.Score.Scaled.Should().Be(0.92m);
            dto.Result.Success.Should().BeTrue();
            dto.Result.Duration.Should().Be("PT4M30S");
            dto.Context.Registration.Should().Be("15dd2c52-c5b3-4e1c-99f1-2c4e7a3a85b3");
            dto.Context.Platform.Should().Be("Febris PC Launcher V3");
            dto.Version.Should().Be("1.0.3");
        }

        [Fact]
        public void XApiStatementDto_VendorExtensions_LandInExtensionData()
        {
            // The two `febris:*` fields aren't modeled on XApiStatementDto.
            // They must end up in ExtensionData for round-trip preservation.
            var dto = JsonConvert.DeserializeObject<XApiStatementDto>(SampleStatementJson);

            dto.ExtensionData.Should().NotBeNull();
            dto.ExtensionData.Should().ContainKey("febris:trainingStationId");
            dto.ExtensionData["febris:trainingStationId"].ToString().Should().Be("station-42");
            dto.ExtensionData.Should().ContainKey("febris:hardwareSerial");
            dto.ExtensionData["febris:hardwareSerial"].ToString().Should().Be("XR-9001-A");
        }

        [Fact]
        public void XApiStatementDto_VendorExtensions_RoundTripPreservesValues()
        {
            // Deserialize then re-serialize. The vendor-extension fields
            // must appear in the re-serialized JSON. This is the
            // defense-in-depth that Option A's raw-bytes capture relies on
            // -- even if a downstream code path serializes from the DTO
            // (rather than the raw bytes), the extension data survives.
            var dto = JsonConvert.DeserializeObject<XApiStatementDto>(SampleStatementJson);
            var reserialized = JsonConvert.SerializeObject(dto);
            var roundTripped = JObject.Parse(reserialized);

            roundTripped["febris:trainingStationId"]?.ToString().Should().Be("station-42");
            roundTripped["febris:hardwareSerial"]?.ToString().Should().Be("XR-9001-A");
        }

        // ---------- ReadAsync happy path -----------------------------

        [Fact]
        public async Task ReadAsync_HappyPath_CapturesRawBytesAndBindsDto()
        {
            var bodyBytes = Encoding.UTF8.GetBytes(SampleStatementJson);
            var req = BuildRequest(bodyBytes, "application/json; charset=utf-8");

            var submission = await XApiStatementBinding.ReadAsync(req);

            submission.DtoBound.Should().BeTrue();
            submission.ParseError.Should().BeNull();
            submission.Dto.Actor.Mbox.Should().Be("mailto:alice@example.com");

            // Critical assertion: raw bytes are byte-for-byte equal to the
            // input stream. This is the "Option A audit-grade preservation"
            // guarantee. If this fails, the audit trail is corrupted.
            submission.RawBody.Should().Equal(bodyBytes);
            submission.RawBody.Length.Should().Be(bodyBytes.Length);

            submission.ContentType.Should().Be("application/json; charset=utf-8");
            // Encoding.GetEncoding("utf-8").WebName == "utf-8".
            submission.RawBodyEncoding.Should().Be("utf-8");
        }

        [Fact]
        public async Task ReadAsync_NoCharsetOnContentType_DefaultsToUtf8()
        {
            var bodyBytes = Encoding.UTF8.GetBytes(SampleStatementJson);
            var req = BuildRequest(bodyBytes, "application/json");

            var submission = await XApiStatementBinding.ReadAsync(req);

            submission.DtoBound.Should().BeTrue();
            submission.RawBodyEncoding.Should().Be("utf-8");
        }

        // ---------- ReadAsync failure modes --------------------------

        [Fact]
        public async Task ReadAsync_MalformedJson_ReportsParseErrorAndKeepsBytes()
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes("{ this is not valid json");
            var req = BuildRequest(bodyBytes, "application/json");

            var submission = await XApiStatementBinding.ReadAsync(req);

            submission.DtoBound.Should().BeFalse();
            submission.Dto.Should().BeNull();
            submission.ParseError.Should().NotBeNullOrEmpty();
            // Even on parse failure we keep the bytes -- "what did the
            // client send" is the whole point.
            submission.RawBody.Should().Equal(bodyBytes);
        }

        [Fact]
        public async Task ReadAsync_EmptyBody_ReportsParseError()
        {
            var req = BuildRequest(new byte[0], "application/json");

            var submission = await XApiStatementBinding.ReadAsync(req);

            submission.DtoBound.Should().BeFalse();
            submission.ParseError.Should().Contain("Empty");
        }

        [Fact]
        public async Task ReadAsync_LiteralNullBody_ReportsParseError()
        {
            // Some clients send `null` as the JSON body when they have
            // no statement to submit. Treat that as a 400-worthy parse
            // failure rather than silently passing null to the BLL.
            var bodyBytes = Encoding.UTF8.GetBytes("null");
            var req = BuildRequest(bodyBytes, "application/json");

            var submission = await XApiStatementBinding.ReadAsync(req);

            submission.DtoBound.Should().BeFalse();
            submission.ParseError.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReadAsync_BodyExceedsCap_RejectsCleanly()
        {
            // Build a body larger than our small test cap.
            var largeBytes = new byte[2048];
            for (int i = 0; i < largeBytes.Length; i++) largeBytes[i] = (byte)'a';
            var req = BuildRequest(largeBytes, "application/json");

            // Set the cap below the body size to trigger the limit branch.
            var submission = await XApiStatementBinding.ReadAsync(req, maxBodyBytes: 1024);

            submission.DtoBound.Should().BeFalse();
            submission.RawBody.Should().BeNull();
            submission.ParseError.Should().Contain("exceeded");
        }

        // ---------- PersistRawBytesAsync -----------------------------

        [Fact]
        public async Task PersistRawBytesAsync_RoundTripsBytesByteForByte()
        {
            // Phase 3.3c audit-trail value: bytes captured by ReadAsync
            // are written to disk verbatim, no encoding round-trip.
            byte[] bytes = Encoding.UTF8.GetBytes(SampleStatementJson);
            var uuid = Guid.NewGuid();
            string tmpDir = Path.Combine(Path.GetTempPath(), "FebrisRawBytesTest_" + Guid.NewGuid().ToString("N"));

            try
            {
                bool ok = await XApiStatementBinding.PersistRawBytesAsync(bytes, uuid, tmpDir);
                ok.Should().BeTrue();

                // T4: the suffix is ".raw.json", not ".json". It used to be plain ".json", which is
                // byte-for-byte the name StatementLogic.SavingJSONStatement writes through
                // IStatementFileHandler.UploadPackage, into the SAME directory, also truncating.
                // Both ran for one statement on /Submit and this write landed second, so the
                // normalized copy was destroyed on every accepted statement.
                string expectedPath = Path.Combine(tmpDir, uuid + XApiStatementBinding.RawBodyFileSuffix);
                File.Exists(expectedPath).Should().BeTrue();

                byte[] readBack = File.ReadAllBytes(expectedPath);
                readBack.Should().Equal(bytes);

                // The name the legacy writer owns must be left alone by this one.
                File.Exists(Path.Combine(tmpDir, uuid + ".json")).Should().BeFalse(
                    "the verbatim copy must not occupy the filename SavingJSONStatement writes, or one destroys the other");
            }
            finally
            {
                if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, recursive: true);
            }
        }

        [Fact]
        public async Task PersistRawBytesAsync_NullBytes_ReturnsFalse()
        {
            (await XApiStatementBinding.PersistRawBytesAsync(null, Guid.NewGuid(), Path.GetTempPath()))
                .Should().BeFalse();
        }

        [Fact]
        public async Task PersistRawBytesAsync_EmptyUuid_ReturnsFalse()
        {
            (await XApiStatementBinding.PersistRawBytesAsync(new byte[] { 1 }, Guid.Empty, Path.GetTempPath()))
                .Should().BeFalse();
        }

        // ---------- helpers ------------------------------------------

        /// <summary>
        /// Build a mock HttpRequest with the given body bytes + content
        /// type. HttpRequest is abstract; Moq fakes the surface we need
        /// (Body, ContentType). No DefaultHttpContext required -- keeps
        /// the test project free of the AspNetCore.Http runtime.
        /// </summary>
        private static HttpRequest BuildRequest(byte[] bodyBytes, string contentType)
        {
            var stream = new MemoryStream(bodyBytes);
            var mock = new Mock<HttpRequest>();
            mock.SetupGet(r => r.Body).Returns(stream);
            mock.SetupGet(r => r.ContentType).Returns(contentType);
            return mock.Object;
        }
    }
}
