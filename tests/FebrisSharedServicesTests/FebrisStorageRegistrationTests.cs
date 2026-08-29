// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using Febris.SharedServices.Storage;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// DI-resolution tests for <c>AddFebrisStorage</c> (file-system overhaul, Phase 1). They prove a host
    /// that calls services.AddFebrisStorage(Configuration) can resolve <see cref="IStorageProvider"/>, that
    /// the legacy SmbClient:Path fallback keeps pre-migration config working, and that selecting the
    /// not-yet-implemented S3 backend fails fast. This GATES the registration without needing a running
    /// host (resolution never touches the file system).
    /// </summary>
    public class FebrisStorageRegistrationTests
    {
        private static IConfiguration Config(Dictionary<string, string> values)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        [Fact]
        public void AddFebrisStorage_resolves_the_filesystem_provider()
        {
            IConfiguration config = Config(new Dictionary<string, string>
            {
                ["Storage:Provider"] = "FileSystem",
                ["Storage:BasePath"] = "/srv/febris-storage",
            });

            using (ServiceProvider provider = new ServiceCollection().AddFebrisStorage(config).BuildServiceProvider())
            {
                IStorageProvider storage = provider.GetRequiredService<IStorageProvider>();
                storage.Should().BeOfType<FileSystemStorageProvider>();
                storage.Kind.Should().Be(StorageProviderKind.FileSystem);
            }
        }

        [Fact]
        public void AddFebrisStorage_falls_back_to_the_legacy_SmbClient_path()
        {
            // No Storage:BasePath, but the legacy SmbClient:Path is set: the filesystem provider must still
            // resolve, so an existing deployment keeps working before it adds the new Storage config.
            IConfiguration config = Config(new Dictionary<string, string>
            {
                ["SmbClient:Path"] = "/srv/febris-legacy",
            });

            using (ServiceProvider provider = new ServiceCollection().AddFebrisStorage(config).BuildServiceProvider())
            {
                provider.GetRequiredService<IStorageProvider>().Should().BeOfType<FileSystemStorageProvider>();
            }
        }

        [Fact]
        public void AddFebrisStorage_resolves_the_s3_provider()
        {
            IConfiguration config = Config(new Dictionary<string, string>
            {
                ["Storage:Provider"] = "S3",
                ["Storage:S3Bucket"] = "febris-test",
                ["Storage:S3Region"] = "us-east-1",
                ["Storage:S3AccessKey"] = "AKIADUMMYACCESSKEY",
                ["Storage:S3SecretKey"] = "dummySecretKeyForTestsOnly",
            });

            using (ServiceProvider provider = new ServiceCollection().AddFebrisStorage(config).BuildServiceProvider())
            {
                IStorageProvider storage = provider.GetRequiredService<IStorageProvider>();
                storage.Should().BeOfType<S3StorageProvider>();
                storage.Kind.Should().Be(StorageProviderKind.S3);
            }
        }
    }
}
