// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// One stored binary artifact in the NODE's own artifact store, created by the delivery-path
    /// severance: a module <c>.zip</c> or a client-software package (mobile Server APK, Companion
    /// APK, PC launcher installer, integration SDK) ingested through
    /// <c>IStorageProvider</c>.
    /// <para>
    /// The catalog rows (<see cref="Module"/> / <see cref="LocalSoftwarePackage"/>) stay untouched
    /// -- they are shared with the central hub schema, so ingest bookkeeping lives HERE instead of
    /// as new columns on them (adding columns would silently drift the central DataDb model). An
    /// artifact row records where the bytes live (<see cref="StorageKey"/>), their SHA-256, and
    /// their length; the row's existence for a catalog item's conventional key is what marks that
    /// item as store-ingested (vs served off the legacy file layout). Mapped ONLY in the tenant
    /// (EndUser) DataDbContext -- central never carries this table.
    /// </para>
    /// </summary>
    public class PackageArtifact : BaseModel
    {
        /// <summary>
        /// Logical, forward-slash key of the stored object relative to the deployment storage root
        /// (for example "modules/{moduleUuid}.zip" or "localsoftwarepackage/{packageUuid}.zip"),
        /// exactly as consumed by <c>IStorageProvider</c>. Unique per artifact: re-ingesting the
        /// same key overwrites the object and updates this row.
        /// </summary>
        [Display(Name = "Storage key")]
        public string StorageKey { get; set; }

        /// <summary>Lowercase hex SHA-256 of the stored bytes, computed from the store after write.</summary>
        [Display(Name = "SHA-256 checksum")]
        public string Sha256 { get; set; }

        /// <summary>Byte length of the stored object.</summary>
        [Display(Name = "Content length")]
        public long ContentLength { get; set; }

        /// <summary>The client-supplied file name at upload time (diagnostics only; never used as a path).</summary>
        [Display(Name = "Original file name")]
        public string SourceFileName { get; set; }
    }
}
