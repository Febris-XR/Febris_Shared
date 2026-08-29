// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.IO;
using System.Threading.Tasks;
using Febris.SharedServices.Storage;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Phase 3 tests for <see cref="StorageKeys"/> (the layout reconciliation phase of the filesystem
    /// overhaul). The headline case proves the key-to-legacy-path reconciliation for
    /// modules: the clean "modules/{file}" key, under a provider rooted at the deployment BaseFileSystemPath,
    /// lands at the exact location the legacy FileInitalizer used (BaseFileSystemPath + "modules/"), so the
    /// module call-site migration is non-breaking with no data move.
    /// </summary>
    public class StorageKeysTests : IDisposable
    {
        private readonly string _basePath;

        public StorageKeysTests()
        {
            _basePath = Path.Combine(
                Path.GetTempPath(),
                "febris-storage-keys-tests",
                Guid.NewGuid().ToString("N"));
        }

        public void Dispose()
        {
            if (Directory.Exists(_basePath))
            {
                Directory.Delete(_basePath, recursive: true);
            }
        }

        [Fact]
        public void Combine_joins_prefix_and_file_with_a_single_forward_slash()
        {
            StorageKeys.Combine("media/images", "a.png").Should().Be("media/images/a.png");
            StorageKeys.Combine("/modules/", "/x.zip").Should().Be("modules/x.zip");
            StorageKeys.Combine("modules", "sub\\y.zip").Should().Be("modules/sub/y.zip");
        }

        [Fact]
        public void Module_key_is_the_clean_modules_prefix()
        {
            StorageKeys.Module("abc123.zip").Should().Be("modules/abc123.zip");
        }

        [Fact]
        public async Task Module_clean_key_lands_at_the_legacy_module_path()
        {
            // The legacy FileInitalizer sets StaticDetails.ModuleFileSystemPath = BaseFileSystemPath +
            // "modules/", so a module file lived at {Base}/modules/{uuid}.zip. With the provider rooted at
            // BaseFileSystemPath, the clean key must resolve to that same on-disk location.
            var provider = new FileSystemStorageProvider(new StorageOptions
            {
                Provider = StorageProviderKind.FileSystem,
                BasePath = _basePath,
            });

            string uuid = "11111111-1111-1111-1111-111111111111";
            string key = StorageKeys.Module(uuid + ".zip");

            await provider.WriteAsync(key, new MemoryStream(new byte[] { 1, 2, 3, 4 }));

            // Exactly where the legacy ModuleFileSystemPath would have put it.
            string legacyLocation = Path.Combine(_basePath, "modules", uuid + ".zip");
            File.Exists(legacyLocation).Should().BeTrue();
        }
    }
}
