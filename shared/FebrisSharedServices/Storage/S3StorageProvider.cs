// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Febris.SharedServices.Storage
{
    /// <summary>
    /// S3-compatible object-store implementation of <see cref="IStorageProvider"/> (file-system overhaul,
    /// Phase 4). One provider serves both AWS S3 and any S3-compatible store (MinIO, etc.): set
    /// <see cref="StorageOptions.S3Endpoint"/> for a non-AWS endpoint, otherwise it talks to AWS in the
    /// configured region. Credentials come from <see cref="StorageOptions.S3AccessKey"/> /
    /// <see cref="StorageOptions.S3SecretKey"/> when set, otherwise the ambient AWS credential chain
    /// (environment, instance profile, etc.).
    /// <para>
    /// A logical key IS the S3 object key directly. Object stores have no real directories, so there is no
    /// legacy-layout reconciliation here (that is a file-system-only concern):
    /// on S3 the clean normalized key layout is the natural
    /// state. <see cref="EnsureAreaAsync"/> is therefore a no-op, and <see cref="GetServeUri"/> returns a
    /// time-limited pre-signed URL so large media serves from the store / CDN, not through the API process.
    /// </para>
    /// </summary>
    public sealed class S3StorageProvider : IStorageProvider
    {
        private readonly IAmazonS3 _client;
        private readonly string _bucket;
        private readonly bool _useHttp;

        public S3StorageProvider(StorageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.S3Bucket))
            {
                throw new InvalidOperationException("Storage:S3Bucket is required for the S3 provider.");
            }

            _bucket = options.S3Bucket;

            var config = new AmazonS3Config();
            if (!string.IsNullOrWhiteSpace(options.S3Endpoint))
            {
                // Non-AWS S3-compatible endpoint (MinIO, etc.). Path-style addressing avoids needing a
                // bucket-name DNS entry on a self-hosted endpoint.
                config.ServiceURL = options.S3Endpoint;
                config.ForcePathStyle = true;
                // A plaintext endpoint (a local/dev MinIO) must produce http pre-signed URLs, otherwise a
                // fetch hits an SSL handshake against a plaintext server.
                _useHttp = options.S3Endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
            }
            else if (!string.IsNullOrWhiteSpace(options.S3Region))
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.S3Region);
            }

            if (!string.IsNullOrWhiteSpace(options.S3AccessKey) && !string.IsNullOrWhiteSpace(options.S3SecretKey))
            {
                _client = new AmazonS3Client(new BasicAWSCredentials(options.S3AccessKey, options.S3SecretKey), config);
            }
            else
            {
                // Ambient credential chain (env vars, ECS/EC2 instance profile, shared credentials file).
                _client = new AmazonS3Client(config);
            }
        }

        public StorageProviderKind Kind => StorageProviderKind.S3;

        public async Task<Stream> OpenReadAsync(string key)
        {
            // GetObjectAsync throws AmazonS3Exception (404 NoSuchKey) when the object is absent, satisfying
            // the "throws if absent" contract.
            GetObjectResponse response = await _client.GetObjectAsync(_bucket, NormalizeKey(key));
            return response.ResponseStream;
        }

        public async Task WriteAsync(string key, Stream content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var request = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = NormalizeKey(key),
                InputStream = content,
                // The caller owns the content stream, matching the file-system provider.
                AutoCloseStream = false,
            };

            await _client.PutObjectAsync(request);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                await _client.GetObjectMetadataAsync(_bucket, NormalizeKey(key));
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task DeleteAsync(string key)
        {
            // S3 delete is idempotent: deleting an absent key is a success, matching the contract no-op.
            await _client.DeleteObjectAsync(_bucket, NormalizeKey(key));
        }

        public async Task<long> GetLengthAsync(string key)
        {
            // GetObjectMetadataAsync throws (404) when absent, satisfying "throws if absent".
            GetObjectMetadataResponse meta = await _client.GetObjectMetadataAsync(_bucket, NormalizeKey(key));
            return meta.ContentLength;
        }

        public async Task<IReadOnlyList<string>> ListAsync(string prefix, string pattern = null)
        {
            var keys = new List<string>();
            Regex matcher = pattern == null ? null : GlobToRegex(pattern);

            var request = new ListObjectsV2Request
            {
                BucketName = _bucket,
                Prefix = NormalizePrefix(prefix),
            };

            ListObjectsV2Response response;
            do
            {
                response = await _client.ListObjectsV2Async(request);
                foreach (S3Object item in response.S3Objects)
                {
                    if (matcher == null || matcher.IsMatch(FileName(item.Key)))
                    {
                        keys.Add(item.Key);
                    }
                }

                request.ContinuationToken = response.NextContinuationToken;
            }
            while (response.IsTruncated);

            return keys;
        }

        public async Task MoveAsync(string sourceKey, string destKey)
        {
            string source = NormalizeKey(sourceKey);
            string dest = NormalizeKey(destKey);

            await _client.CopyObjectAsync(new CopyObjectRequest
            {
                SourceBucket = _bucket,
                SourceKey = source,
                DestinationBucket = _bucket,
                DestinationKey = dest,
            });

            await _client.DeleteObjectAsync(_bucket, source);
        }

        public Task EnsureAreaAsync(string areaPrefix)
        {
            // Object stores have no directories: prefixes spring into existence when an object is written.
            return Task.CompletedTask;
        }

        public Uri GetServeUri(string key, TimeSpan ttl)
        {
            string url = _client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = NormalizeKey(key),
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(ttl),
                Protocol = _useHttp ? Protocol.HTTP : Protocol.HTTPS,
            });

            return new Uri(url);
        }

        /// <summary>Trim leading separators so a key is always relative, and use forward slashes (S3 native).</summary>
        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A storage key is required.", nameof(key));
            }

            return key.TrimStart('/', '\\').Replace('\\', '/');
        }

        /// <summary>
        /// Scope a list to an area: an empty prefix lists the whole bucket, otherwise list keys under the
        /// prefix folder (trailing slash) so "modules" does not also catch "modules-archive".
        /// </summary>
        private static string NormalizePrefix(string prefix)
        {
            string trimmed = (prefix ?? string.Empty).Trim('/', '\\').Replace('\\', '/');
            return trimmed.Length == 0 ? null : trimmed + "/";
        }

        private static string FileName(string key)
        {
            int slash = key.LastIndexOf('/');
            return slash < 0 ? key : key.Substring(slash + 1);
        }

        private static Regex GlobToRegex(string pattern)
        {
            var builder = new StringBuilder("^");
            foreach (char c in pattern)
            {
                if (c == '*')
                {
                    builder.Append(".*");
                }
                else if (c == '?')
                {
                    builder.Append('.');
                }
                else
                {
                    builder.Append(Regex.Escape(c.ToString()));
                }
            }

            builder.Append('$');
            return new Regex(builder.ToString(), RegexOptions.IgnoreCase);
        }
    }
}
