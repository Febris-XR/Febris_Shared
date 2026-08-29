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
    /// Phase 2 tests for <see cref="StorageInitializer.EnsureHostAreasAsync"/>. They run host init through a
    /// <see cref="FileSystemStorageProvider"/> pointed at a fresh temp directory, then assert the resulting
    /// directory tree. The headline case proves the EndUser scoping fix: an EndUser host materializes its own
    /// areas but NEVER logs/adminportal.
    /// </summary>
    public class StorageInitializerTests : IDisposable
    {
        private readonly string _basePath;
        private readonly IStorageProvider _provider;

        public StorageInitializerTests()
        {
            _basePath = Path.Combine(
                Path.GetTempPath(),
                "febris-storage-init-tests",
                Guid.NewGuid().ToString("N"));

            _provider = new FileSystemStorageProvider(new StorageOptions
            {
                Provider = StorageProviderKind.FileSystem,
                BasePath = _basePath,
            });
        }

        // Each test gets its own temp root (fresh Guid per instance), so deleting it here keeps the
        // suite self-cleaning and deterministic.
        public void Dispose()
        {
            if (Directory.Exists(_basePath))
            {
                Directory.Delete(_basePath, recursive: true);
            }
        }

        // Map a logical forward-slash key prefix to its OS directory under the base path.
        private string AreaDirectory(string keyPrefix)
        {
            return Path.Combine(_basePath, keyPrefix.Replace('/', Path.DirectorySeparatorChar));
        }

        [Fact]
        public async Task EndUser_manifest_creates_its_areas_and_NOT_adminportal()
        {
            await StorageInitializer.EnsureHostAreasAsync(_provider, StorageManifests.EndUser);

            // The EndUser host's own areas must exist.
            Directory.Exists(AreaDirectory("modules")).Should().BeTrue();
            Directory.Exists(AreaDirectory("statements/json")).Should().BeTrue();
            Directory.Exists(AreaDirectory("logs/api")).Should().BeTrue();
            Directory.Exists(AreaDirectory("logs/portal")).Should().BeTrue();
            Directory.Exists(AreaDirectory("media/images")).Should().BeTrue();

            // The EndUser deployment must NOT materialize central-only areas it does not use locally: the
            // admin-portal log dir, the marketplace listing tree, content-developer logos, and -- since
            // the ROADMAP 17 reachability sweep deleted the WidgetController media loaders that were the
            // only node code serving them -- badges, publications, and email-campaign assets.
            Directory.Exists(Path.Combine(_basePath, "logs", "adminportal")).Should().BeFalse();
            Directory.Exists(AreaDirectory("marketplace/listings")).Should().BeFalse();
            Directory.Exists(AreaDirectory("media/images/developerlogos")).Should().BeFalse();
            Directory.Exists(AreaDirectory("media/images/badges")).Should().BeFalse();
            Directory.Exists(AreaDirectory("publications")).Should().BeFalse();
            Directory.Exists(AreaDirectory("emailcampaign")).Should().BeFalse();
        }

        [Fact]
        public async Task Central_manifest_creates_adminportal()
        {
            await StorageInitializer.EnsureHostAreasAsync(_provider, StorageManifests.Central);

            // A central host owns the adminportal log area, so it must exist.
            Directory.Exists(Path.Combine(_basePath, "logs", "adminportal")).Should().BeTrue();
        }

        [Fact]
        public async Task EnsureHostAreasAsync_is_idempotent()
        {
            await StorageInitializer.EnsureHostAreasAsync(_provider, StorageManifests.EndUser);

            // Running host init a second time over already-created areas must not throw.
            Func<Task> again = () => StorageInitializer.EnsureHostAreasAsync(_provider, StorageManifests.EndUser);
            await again.Should().NotThrowAsync();
        }
    }
}
