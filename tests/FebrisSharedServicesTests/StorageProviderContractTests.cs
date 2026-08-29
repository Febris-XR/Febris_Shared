// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Febris.SharedServices.Storage;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Phase 0 conformance suite for <see cref="IStorageProvider"/>. It exercises the contract through
    /// <see cref="FileSystemStorageProvider"/> pointed at a fresh temp directory, so it runs in CI
    /// with no real DB or host. Every later provider (S3, etc.) must pass the same suite.
    ///
    /// <para>
    /// A key is a logical, forward-slash path relative to the deployment storage root, for example
    /// "media/images/a.png". Keys never contain absolute paths, drive letters, or backend
    /// assumptions, and providers must reject keys that escape the root (a "../" traversal).
    /// </para>
    /// </summary>
    public class StorageProviderContractTests : IDisposable
    {
        private readonly string _basePath;
        private readonly StorageOptions _options;
        private readonly IStorageProvider _provider;

        public StorageProviderContractTests()
        {
            _basePath = Path.Combine(
                Path.GetTempPath(),
                "febris-storage-tests",
                Guid.NewGuid().ToString("N"));

            _options = new StorageOptions
            {
                Provider = StorageProviderKind.FileSystem,
                BasePath = _basePath,
            };

            _provider = new FileSystemStorageProvider(_options);
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

        private static MemoryStream Bytes(params byte[] payload) => new MemoryStream(payload);

        private static async Task<byte[]> ReadAllAsync(Stream stream)
        {
            using (stream)
            using (var buffer = new MemoryStream())
            {
                await stream.CopyToAsync(buffer);
                return buffer.ToArray();
            }
        }

        [Fact]
        public void Kind_is_FileSystem()
        {
            _provider.Kind.Should().Be(StorageProviderKind.FileSystem);
        }

        [Fact]
        public async Task Write_then_OpenRead_round_trips()
        {
            byte[] payload = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

            await _provider.WriteAsync("media/images/a.png", Bytes(payload));
            byte[] readBack = await ReadAllAsync(await _provider.OpenReadAsync("media/images/a.png"));

            readBack.Should().Equal(payload);
        }

        [Fact]
        public async Task Exists_is_false_before_and_true_after_write()
        {
            (await _provider.ExistsAsync("media/images/a.png")).Should().BeFalse();

            await _provider.WriteAsync("media/images/a.png", Bytes(1, 2, 3));

            (await _provider.ExistsAsync("media/images/a.png")).Should().BeTrue();
        }

        [Fact]
        public async Task GetLength_matches_written_byte_count()
        {
            byte[] payload = { 1, 2, 3, 4, 5, 6, 7 };

            await _provider.WriteAsync("modules/sample.zip", Bytes(payload));

            (await _provider.GetLengthAsync("modules/sample.zip")).Should().Be(payload.Length);
        }

        [Fact]
        public async Task Delete_removes_object_and_is_noop_when_absent()
        {
            await _provider.WriteAsync("media/images/a.png", Bytes(1, 2, 3));
            (await _provider.ExistsAsync("media/images/a.png")).Should().BeTrue();

            await _provider.DeleteAsync("media/images/a.png");
            (await _provider.ExistsAsync("media/images/a.png")).Should().BeFalse();

            // Deleting an absent object is a contract no-op, not an error.
            Func<Task> deleteAgain = () => _provider.DeleteAsync("media/images/a.png");
            await deleteAgain.Should().NotThrowAsync();
        }

        [Fact]
        public async Task List_returns_logical_forward_slash_keys_under_prefix()
        {
            await _provider.WriteAsync("modules/alpha.zip", Bytes(1));
            await _provider.WriteAsync("modules/beta.zip", Bytes(2));
            await _provider.WriteAsync("modules/nested/gamma.zip", Bytes(3));

            var keys = await _provider.ListAsync("modules");

            // Listings are logical forward-slash keys relative to the root, never absolute paths.
            keys.Should().Contain("modules/alpha.zip");
            keys.Should().Contain("modules/beta.zip");
            keys.Should().Contain("modules/nested/gamma.zip");
            keys.Should().OnlyContain(k => !Path.IsPathRooted(k));
            keys.Should().OnlyContain(k => !k.Contains("\\"));
        }

        [Fact]
        public async Task Move_relocates_object()
        {
            byte[] payload = { 10, 20, 30, 40 };

            await _provider.WriteAsync("modules/incoming/pkg.zip", Bytes(payload));

            await _provider.MoveAsync("modules/incoming/pkg.zip", "modules/published/pkg.zip");

            (await _provider.ExistsAsync("modules/incoming/pkg.zip")).Should().BeFalse();
            (await _provider.ExistsAsync("modules/published/pkg.zip")).Should().BeTrue();

            byte[] readBack = await ReadAllAsync(await _provider.OpenReadAsync("modules/published/pkg.zip"));
            readBack.Should().Equal(payload);
        }

        [Fact]
        public async Task EnsureArea_is_idempotent()
        {
            await _provider.EnsureAreaAsync("media/images");

            // Calling again on an area that already exists must not throw.
            Func<Task> again = () => _provider.EnsureAreaAsync("media/images");
            await again.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Write_creates_intermediate_area_without_pre_ensuring()
        {
            byte[] payload = { 7, 7, 7 };

            // No EnsureAreaAsync first: WriteAsync must create any intermediate area (mkdir -p).
            await _provider.WriteAsync("media/images/logos/deep/nested/x.png", Bytes(payload));

            (await _provider.ExistsAsync("media/images/logos/deep/nested/x.png")).Should().BeTrue();
            byte[] readBack = await ReadAllAsync(
                await _provider.OpenReadAsync("media/images/logos/deep/nested/x.png"));
            readBack.Should().Equal(payload);
        }

        [Fact]
        public async Task Resolve_rejects_traversal_key()
        {
            // A "../" key escapes the deployment root and must be rejected on both read and write.
            Func<Task> read = () => _provider.OpenReadAsync("../escape.txt");
            Func<Task> write = () => _provider.WriteAsync("../escape.txt", Bytes(1, 2, 3));

            await read.Should().ThrowAsync<Exception>();
            await write.Should().ThrowAsync<Exception>();
        }
    }
}
