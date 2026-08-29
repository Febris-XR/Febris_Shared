// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Febris.SharedServices.Storage
{
    /// <summary>
    /// File-system overhaul (Phase 1): the single storage registration entry point, called once per
    /// host as services.AddFebrisStorage(Configuration). It binds the "Storage" configuration section
    /// to <see cref="StorageOptions"/>, selects the backend from "Storage:Provider", and registers the
    /// chosen <see cref="IStorageProvider"/> plus the bound options as singletons. Mirrors
    /// AddFebrisDataAccess.
    /// </summary>
    public static class FebrisStorageRegistration
    {
        public static IServiceCollection AddFebrisStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();

            // Migration aid: a deployment that has not added the new "Storage" config yet keeps working.
            // When the file-system backend is selected with no BasePath, fall back to the legacy file
            // root ("SmbClient:Path") so the existing SMB / local mount is used unchanged until the
            // deployment opts in to the new configuration.
            if (options.Provider == StorageProviderKind.FileSystem && string.IsNullOrWhiteSpace(options.BasePath))
            {
                options.BasePath = configuration["SmbClient:Path"];
            }

            // Register the bound options as a singleton so callers (and the provider) can resolve them.
            services.AddSingleton(options);

            switch (options.Provider)
            {
                case StorageProviderKind.FileSystem:
                    // Lazy factory: the provider is constructed on first resolve, not at registration, so
                    // wiring AddFebrisStorage into a host before anything uses storage cannot crash startup
                    // on a missing/blank BasePath. A bad BasePath surfaces on the first file operation.
                    services.AddSingleton<IStorageProvider>(_ => new FileSystemStorageProvider(options));
                    break;
                case StorageProviderKind.S3:
                    // Lazy factory: the S3 client is built on first resolve, not at registration.
                    services.AddSingleton<IStorageProvider>(_ => new S3StorageProvider(options));
                    break;
                default:
                    throw new NotSupportedException("Unknown Storage:Provider value. Set Storage:Provider=FileSystem.");
            }

            return services;
        }
    }
}
