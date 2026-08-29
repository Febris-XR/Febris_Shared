// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Febris.SharedServices.Storage;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="S3StorageProvider"/> (file-system overhaul, Phase 4). Construction and pre-signed
    /// URL generation are verifiable offline (SigV4 is local HMAC, no network). The full IStorageProvider
    /// conformance round-trip is a SkippableFact that runs against a live S3 / MinIO endpoint when configured
    /// by environment, otherwise it is skipped (CI-safe). Set:
    ///   FEBRIS_S3_TEST_ENDPOINT   (e.g. http://localhost:9000 for MinIO; omit for AWS)
    ///   FEBRIS_S3_TEST_BUCKET     (e.g. febris-storage-test)
    ///   FEBRIS_S3_TEST_ACCESSKEY / FEBRIS_S3_TEST_SECRETKEY
    /// </summary>
    public class S3StorageProviderTests
    {
        private static StorageOptions DummyOptions()
        {
            return new StorageOptions
            {
                Provider = StorageProviderKind.S3,
                S3Bucket = "febris-test",
                S3Region = "us-east-1",
                S3AccessKey = "AKIADUMMYACCESSKEY",
                S3SecretKey = "dummySecretKeyForTestsOnly",
            };
        }

        [Fact]
        public void Kind_is_S3()
        {
            new S3StorageProvider(DummyOptions()).Kind.Should().Be(StorageProviderKind.S3);
        }

        [Fact]
        public void Missing_bucket_throws()
        {
            Action act = () => new S3StorageProvider(new StorageOptions { Provider = StorageProviderKind.S3 });
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetServeUri_returns_a_presigned_url_for_the_key()
        {
            // SigV4 pre-signing is local crypto with no network call, so this is verifiable without a bucket.
            var provider = new S3StorageProvider(DummyOptions());

            Uri uri = provider.GetServeUri("modules/abc.zip", TimeSpan.FromMinutes(5));

            uri.Should().NotBeNull();
            uri.AbsoluteUri.Should().Contain("febris-test");
            uri.AbsoluteUri.Should().Contain("modules/abc.zip");
            uri.AbsoluteUri.ToLowerInvariant().Should().Contain("signature");
        }

        [SkippableFact]
        public async Task Conformance_round_trip_against_a_live_s3_or_minio_endpoint()
        {
            string endpoint = Environment.GetEnvironmentVariable("FEBRIS_S3_TEST_ENDPOINT");
            string bucket = Environment.GetEnvironmentVariable("FEBRIS_S3_TEST_BUCKET");
            Skip.If(
                string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(bucket),
                "Set FEBRIS_S3_TEST_ENDPOINT and FEBRIS_S3_TEST_BUCKET to run the S3 conformance round-trip.");

            var options = new StorageOptions
            {
                Provider = StorageProviderKind.S3,
                S3Bucket = bucket,
                S3Endpoint = endpoint,
                S3AccessKey = Environment.GetEnvironmentVariable("FEBRIS_S3_TEST_ACCESSKEY"),
                S3SecretKey = Environment.GetEnvironmentVariable("FEBRIS_S3_TEST_SECRETKEY"),
            };

            await EnsureBucketAsync(options);

            var provider = new S3StorageProvider(options);
            string key = "modules/conformance-" + Guid.NewGuid().ToString("N") + ".bin";
            byte[] payload = { 9, 8, 7, 6, 5, 4, 3, 2, 1 };

            // write -> exists -> length
            (await provider.ExistsAsync(key)).Should().BeFalse();
            await provider.WriteAsync(key, new MemoryStream(payload));
            (await provider.ExistsAsync(key)).Should().BeTrue();
            (await provider.GetLengthAsync(key)).Should().Be(payload.Length);

            // read back the bytes
            using (Stream read = await provider.OpenReadAsync(key))
            using (var buffer = new MemoryStream())
            {
                await read.CopyToAsync(buffer);
                buffer.ToArray().Should().Equal(payload);
            }

            // list under the prefix returns the logical key
            (await provider.ListAsync("modules")).Should().Contain(key);

            // move relocates the object
            string moved = "modules/moved-" + Guid.NewGuid().ToString("N") + ".bin";
            await provider.MoveAsync(key, moved);
            (await provider.ExistsAsync(key)).Should().BeFalse();
            (await provider.ExistsAsync(moved)).Should().BeTrue();

            // pre-signed URL actually serves the bytes
            Uri serveUri = provider.GetServeUri(moved, TimeSpan.FromMinutes(5));
            using (var http = new HttpClient())
            {
                byte[] served = await http.GetByteArrayAsync(serveUri);
                served.Should().Equal(payload);
            }

            // cleanup
            await provider.DeleteAsync(moved);
            (await provider.ExistsAsync(moved)).Should().BeFalse();
        }

        private static async Task EnsureBucketAsync(StorageOptions options)
        {
            var config = new AmazonS3Config { ServiceURL = options.S3Endpoint, ForcePathStyle = true };
            using (var admin = new AmazonS3Client(
                new BasicAWSCredentials(options.S3AccessKey, options.S3SecretKey), config))
            {
                try
                {
                    await admin.PutBucketAsync(options.S3Bucket);
                }
                catch (AmazonS3Exception ex) when (
                    ex.StatusCode == System.Net.HttpStatusCode.Conflict
                    || (ex.ErrorCode != null && ex.ErrorCode.StartsWith("BucketAlready")))
                {
                    // Bucket already exists -- fine.
                }
            }
        }
    }
}
