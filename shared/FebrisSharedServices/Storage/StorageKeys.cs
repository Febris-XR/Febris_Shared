// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;

namespace Febris.SharedServices.Storage
{
    /// <summary>
    /// Builds logical storage keys (file-system overhaul, Phase 3). A key is a forward-slash path
    /// relative to the deployment storage root that <see cref="IStorageProvider"/> consumes. This is the
    /// one place call sites construct keys, so no call site hand-builds an absolute path or bakes in a
    /// backend assumption. The key for a file in an area is the area's <see cref="StorageArea.KeyPrefix"/>
    /// joined to the file name.
    /// <para>
    /// RECONCILIATION (read, "Phase 3 layout reconciliation"):
    /// the legacy on-disk layout is NOT uniform. Some areas are rooted directly under
    /// <c>BaseFileSystemPath</c> (modules, marketplace, publications, email-campaign, software packages),
    /// others under <c>SpecificFileSystemPath</c> = Base + the per-deployment Unique segment (media,
    /// statements, logs), and casing varies ("Images", "MarketplaceListings"). A clean lowercase key under
    /// a single Base-rooted provider therefore maps 1:1 to the legacy path ONLY for the Base-rooted,
    /// already-lowercase areas (modules). The Specific-rooted and mixed-case areas need the layout decision
    /// (preserve legacy vs normalize + migrate data) before their builders can be trusted, so only the
    /// verified-clean builders are exposed here for now.
    /// </para>
    /// </summary>
    public static class StorageKeys
    {
        /// <summary>Join an area prefix and a file name into a logical forward-slash key.</summary>
        public static string In(StorageArea area, string fileName)
        {
            if (area == null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            return Combine(area.KeyPrefix, fileName);
        }

        /// <summary>Join a forward-slash prefix and a file name, trimming stray separators.</summary>
        public static string Combine(string prefix, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("A file name is required.", nameof(fileName));
            }

            string left = (prefix ?? string.Empty).Trim('/', '\\');
            string right = fileName.Trim('/', '\\').Replace('\\', '/');

            return left.Length == 0 ? right : left + "/" + right;
        }

        /// <summary>
        /// Key for a module package. VERIFIED non-breaking: modules are Base-rooted and already lowercase
        /// (StaticDetails.ModuleFileSystemPath = BaseFileSystemPath + "modules/"), so this clean key under a
        /// Base-rooted provider resolves to the exact legacy on-disk location with no data move. See
        /// StorageKeysTests.Module_clean_key_lands_at_the_legacy_module_path.
        /// </summary>
        public static string Module(string fileName) => In(StorageAreas.Modules, fileName);

        /// <summary>
        /// Key for a client-software package (mobile Server APK, Companion APK, PC launcher installer,
        /// integration SDK) in the NODE's own software-package store. Safe to use the
        /// clean lowercase area prefix despite the legacy mixed-case "LocalSoftwarePackage/" layout: the
        /// tenant tier NEVER stored software packages locally (it proxied central over HTTP), so the node
        /// store is greenfield -- there is no legacy tenant data to collide with. Central's own legacy
        /// layout is unaffected (central does not use this builder).
        /// </summary>
        public static string SoftwarePackage(string fileName) => In(StorageAreas.LocalSoftwarePackage, fileName);

        /// <summary>
        /// Key for a node-health storage probe object (node health site). The
        /// dedicated <c>healthprobe/</c> prefix is greenfield -- no legacy layout ever wrote
        /// there, so probe writes can never collide with real content areas. Probe objects are
        /// tiny, uniquely named, and deleted by the health check after each round-trip;
        /// providers mkdir-p intermediate containers, so the prefix needs no declared
        /// <see cref="StorageArea"/>.
        /// </summary>
        public static string HealthProbe(string fileName) => Combine("healthprobe", fileName);
    }
}
