// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Febris.SharedServices.Storage
{
    // StorageProviderKind enum moved to Febris.EnumLibrary per the "all enums live in FebrisEnumLibrary" rule.

    /// <summary>
    /// Deployment storage configuration, bound from the "Storage" configuration section by
    /// AddFebrisStorage. Selects the backend and carries the backend-specific settings.
    /// FileSystem is the default so existing deployments behave unchanged until they opt in to S3.
    /// </summary>
    public sealed class StorageOptions
    {
        public StorageProviderKind Provider { get; set; } = StorageProviderKind.FileSystem;

        /// <summary>FileSystem backend: the deployment root (local path or an SMB mount).</summary>
        public string BasePath { get; set; }

        /// <summary>S3 backend (Phase 4): bucket name.</summary>
        public string S3Bucket { get; set; }

        /// <summary>S3 backend (Phase 4): service endpoint (for MinIO / non-AWS S3-compatible).</summary>
        public string S3Endpoint { get; set; }

        /// <summary>S3 backend (Phase 4): region.</summary>
        public string S3Region { get; set; }

        /// <summary>S3 backend (Phase 4): access key id (prefer env / secret injection).</summary>
        public string S3AccessKey { get; set; }

        /// <summary>S3 backend (Phase 4): secret access key (prefer env / secret injection).</summary>
        public string S3SecretKey { get; set; }
    }

    /// <summary>
    /// The storage backend seam (file-system overhaul, Phase 1). Every file the platform stores or
    /// serves goes through this instead of touching absolute paths or FileStream directly, so a
    /// deployment can run on a local/SMB file system or an S3-compatible object store by config
    /// alone.
    /// <para>
    /// A <b>key</b> is a logical, forward-slash path relative to the deployment storage root, for
    /// example "media/images/logos/{guid}.png" or "modules/{id}.zip". Keys never contain absolute
    /// paths, drive letters, or backend assumptions. Implementations map a key to a file path or an
    /// object key. Implementations must reject keys that escape the root (a "../" traversal).
    /// </para>
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>Which backend this instance is (for diagnostics and capability checks).</summary>
        StorageProviderKind Kind { get; }

        /// <summary>Open a readable stream for the object at <paramref name="key"/>. Throws if absent.</summary>
        Task<Stream> OpenReadAsync(string key);

        /// <summary>
        /// Create or replace the object at <paramref name="key"/> from <paramref name="content"/>.
        /// The provider creates any intermediate container/area as needed (mkdir -p on file systems,
        /// implicit on object stores).
        /// </summary>
        Task WriteAsync(string key, Stream content);

        /// <summary>True when an object exists at <paramref name="key"/>.</summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>Delete the object at <paramref name="key"/>. A no-op if it does not exist.</summary>
        Task DeleteAsync(string key);

        /// <summary>Byte length of the object at <paramref name="key"/>. Throws if absent.</summary>
        Task<long> GetLengthAsync(string key);

        /// <summary>
        /// List object keys under <paramref name="prefix"/>, optionally filtered by a simple glob
        /// <paramref name="pattern"/> (for example "*.json"). Returns logical keys, not absolute paths.
        /// </summary>
        Task<IReadOnlyList<string>> ListAsync(string prefix, string pattern = null);

        /// <summary>Move/rename the object from <paramref name="sourceKey"/> to <paramref name="destKey"/>.</summary>
        Task MoveAsync(string sourceKey, string destKey);

        /// <summary>
        /// Ensure the storage area at <paramref name="areaPrefix"/> exists. On a file system this
        /// creates the directory (idempotent); on an object store this is a no-op (prefixes are
        /// implicit). This is what host-scoped startup initialization calls per declared area.
        /// </summary>
        Task EnsureAreaAsync(string areaPrefix);

        /// <summary>
        /// A URI a client can use to fetch the object directly. On an object store this is a
        /// pre-signed, time-limited URL (so large media serves off the store / CDN, not through the
        /// API process); on a file system it is a best-effort local/file URI and
        /// <paramref name="ttl"/> is ignored.
        /// </summary>
        Uri GetServeUri(string key, TimeSpan ttl);
    }
}
