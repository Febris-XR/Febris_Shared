// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Febris.SharedServices.Storage
{
    /// <summary>
    /// Local / SMB file-system implementation of <see cref="IStorageProvider"/> (file-system
    /// overhaul, Phase 1). Re-expresses today's Directory/File/FileStream behavior behind the
    /// logical key model: a key is a forward-slash path relative to <see cref="StorageOptions.BasePath"/>
    /// (the deployment root, a local path or an SMB mount), and this provider maps it to an OS path.
    /// <para>
    /// Every resolved path is confirmed to stay within the base path, so a key with a "../" traversal
    /// can never escape the root (required by the interface contract).
    /// </para>
    /// </summary>
    public sealed class FileSystemStorageProvider : IStorageProvider
    {
        private readonly string _basePath;

        public FileSystemStorageProvider(StorageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.BasePath))
            {
                throw new InvalidOperationException("Storage:BasePath is required for the FileSystem provider.");
            }

            // Fully qualify the root up front so traversal checks compare absolute path to absolute path.
            _basePath = Path.GetFullPath(options.BasePath);
        }

        public StorageProviderKind Kind => StorageProviderKind.FileSystem;

        public Task<Stream> OpenReadAsync(string key)
        {
            string resolved = Resolve(key);
            // File.OpenRead throws FileNotFoundException when absent, which satisfies "throws if absent".
            return Task.FromResult((Stream)File.OpenRead(resolved));
        }

        public async Task WriteAsync(string key, Stream content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string resolved = Resolve(key);

            // Create-or-replace, auto-creating any intermediate area (mkdir -p) just like the contract states.
            string directory = Path.GetDirectoryName(resolved);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fs = new FileStream(resolved, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await content.CopyToAsync(fs);
            }
        }

        public Task<bool> ExistsAsync(string key)
        {
            string resolved = Resolve(key);
            return Task.FromResult(File.Exists(resolved));
        }

        public Task DeleteAsync(string key)
        {
            string resolved = Resolve(key);
            if (File.Exists(resolved))
            {
                File.Delete(resolved);
            }

            // No-op when the file does not exist, per the contract.
            return Task.CompletedTask;
        }

        public Task<long> GetLengthAsync(string key)
        {
            string resolved = Resolve(key);
            // FileInfo.Length throws (FileNotFoundException) when the file is absent, satisfying "throws if absent".
            return Task.FromResult(new FileInfo(resolved).Length);
        }

        public Task<IReadOnlyList<string>> ListAsync(string prefix, string pattern = null)
        {
            string directory = Resolve(prefix);
            if (!Directory.Exists(directory))
            {
                return Task.FromResult((IReadOnlyList<string>)new List<string>());
            }

            string[] absolutePaths = Directory.GetFiles(directory, pattern ?? "*", SearchOption.AllDirectories);

            var keys = new List<string>(absolutePaths.Length);
            foreach (string absolutePath in absolutePaths)
            {
                keys.Add(ToKey(absolutePath));
            }

            return Task.FromResult((IReadOnlyList<string>)keys);
        }

        public Task MoveAsync(string sourceKey, string destKey)
        {
            string sourceResolved = Resolve(sourceKey);
            string destResolved = Resolve(destKey);

            string destDirectory = Path.GetDirectoryName(destResolved);
            if (!string.IsNullOrEmpty(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            // netcoreapp3.1 File.Move has no overwrite overload, so clear an existing destination first.
            if (File.Exists(destResolved))
            {
                File.Delete(destResolved);
            }

            File.Move(sourceResolved, destResolved);
            return Task.CompletedTask;
        }

        public Task EnsureAreaAsync(string areaPrefix)
        {
            string resolved = Resolve(areaPrefix);
            // Idempotent: Directory.CreateDirectory is a no-op when the directory already exists.
            Directory.CreateDirectory(resolved);
            return Task.CompletedTask;
        }

        public Uri GetServeUri(string key, TimeSpan ttl)
        {
            string resolved = Resolve(key);
            // ttl is ignored on the file system (there is nothing to pre-sign): this is a best-effort
            // local file:// URI. Object stores return a time-limited pre-signed URL instead.
            return new Uri(resolved);
        }

        /// <summary>
        /// Map a logical forward-slash key to an absolute OS path under the base path, rejecting any key
        /// that would escape the root (a "../" traversal). Returns the fully qualified path.
        /// </summary>
        private string Resolve(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A storage key is required.", nameof(key));
            }

            // Trim leading separators so the key is always treated as relative to the base path, then
            // map forward-slash key segments to the OS separator.
            string normalized = key.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);

            string fullPath = Path.GetFullPath(Path.Combine(_basePath, normalized));

            // Traversal guard: the resolved path must stay within the base path. Compare with an
            // OS-appropriate case sensitivity (Windows paths are case-insensitive, Unix case-sensitive).
            StringComparison comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            // Append a trailing separator to the base before comparing so "/rootevil" cannot pass as "/root".
            string baseWithSeparator = _basePath.EndsWith(Path.DirectorySeparatorChar.ToString(), comparison)
                ? _basePath
                : _basePath + Path.DirectorySeparatorChar;

            bool withinRoot = fullPath.Equals(_basePath, comparison)
                || fullPath.StartsWith(baseWithSeparator, comparison);

            if (!withinRoot)
            {
                throw new ArgumentException("The storage key escapes the storage root.", nameof(key));
            }

            return fullPath;
        }

        /// <summary>
        /// Map an absolute OS path back to a logical forward-slash key relative to the base path.
        /// </summary>
        private string ToKey(string absolutePath)
        {
            string relative = absolutePath.Substring(_basePath.Length).TrimStart('/', '\\');
            return relative.Replace('\\', '/');
        }
    }
}
