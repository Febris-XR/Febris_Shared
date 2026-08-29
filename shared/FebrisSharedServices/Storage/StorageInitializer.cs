// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Threading.Tasks;

namespace Febris.SharedServices.Storage
{
    /// <summary>
    /// Host-scoped, backend-neutral startup initialization for the file-system overhaul (Phase 2). Given a
    /// host's <see cref="StorageManifest"/>, it walks the declared areas and calls
    /// <see cref="IStorageProvider.EnsureAreaAsync"/> once per area. On a file system that creates each declared
    /// directory (idempotent); on an object store EnsureAreaAsync is a no-op (prefixes are implicit). Because a
    /// host only ever passes its OWN manifest, no host can materialize another host's areas (for example an
    /// EndUser deployment never creates logs/adminportal -- the exact bug Phase 2 fixes).
    /// <para>
    /// This is NOT yet wired into any host. The Startup / FileInitalizer cutover is a later, runtime-gated Phase 2
    /// slice.
    /// </para>
    /// </summary>
    public static class StorageInitializer
    {
        /// <summary>
        /// Ensure every storage area declared in <paramref name="manifest"/> exists, using
        /// <paramref name="provider"/>. Idempotent and backend-neutral: safe to call on every startup.
        /// </summary>
        public static async Task EnsureHostAreasAsync(IStorageProvider provider, StorageManifest manifest)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            foreach (StorageArea area in manifest.Areas)
            {
                await provider.EnsureAreaAsync(area.KeyPrefix);
            }
        }
    }
}
