// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.LookupModels;
using Febris.ModelLibrary.Models.DataModels;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Multipart form payload for the node's module-package ingest endpoint
    /// (POST api/Module/Upload on the EndUser API): one module <c>.zip</c>
    /// plus the catalog metadata for its local Module row. Package format mirrors the legacy
    /// central upload handler: <c>.zip</c> only, stored as <c>modules/{uuid}.zip</c>.
    /// </summary>
    public class ModulePackageUploadViewModel
    {
        /// <summary>
        /// Catalog identity. Supply the module's existing UUID to update its catalog row and
        /// replace its package; omit for a brand-new module (the node assigns one).
        /// </summary>
        public Guid? UUID { get; set; }

        [Display(Name = "Course Name")]
        public string Name { get; set; }

        public string Version { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Language")]
        public LanguageMapTypeEnum Language { get; set; }

        [Display(Name = "Interaction type")]
        public XApiInteractionType XApiInteractionType { get; set; }

        [Display(Name = "Main section count")]
        public int MainSectionCount { get; set; }

        [Display(Name = "All sections and subsection count")]
        public int TotalSectionCount { get; set; }

        [Display(Name = "Interaction components")]
        public string InteractionComponents { get; set; }

        [Display(Name = "Estimated completion time in minutes")]
        public int EstimatedCompletionTime { get; set; }

        /// <summary>
        /// Optional xAPI activity linkage (local Object surrogate key): when supplied, the ingest
        /// upserts the module's ModuleLinkedObject row so statement initialization resolves the
        /// activity locally.
        /// </summary>
        public long? ObjectId { get; set; }

        /// <summary>Optional xAPI activity linkage (Object UUID); paired with <see cref="ObjectId"/>.</summary>
        public Guid? ObjectUUID { get; set; }

        /// <summary>The module package (.zip).</summary>
        public IFormFile File { get; set; }
    }

    /// <summary>
    /// Multipart form payload for the node's client-software ingest endpoint
    /// (POST api/SoftwarePackage/Upload on the EndUser API): one software
    /// package plus the catalog metadata for its local LocalSoftwarePackage row. The KIND reuses
    /// the existing <see cref="LocalSoftwarePackageType"/> enum: PC = the launcher installer,
    /// AndroidMobileServer = the mobile Server APK, AndroidMobileCompanion = the Companion APK,
    /// CSharp / CPP = the integration SDKs. Package format mirrors the legacy central handler:
    /// <c>.zip</c> only (APKs/installers ship zip-wrapped, exactly as the mobile Server's existing
    /// CompanionApp download path already expects), stored as <c>localsoftwarepackage/{uuid}.zip</c>.
    /// </summary>
    public class SoftwarePackageUploadViewModel
    {
        /// <summary>
        /// Catalog identity. Supply an existing package UUID to update it in place; omit for a new
        /// package version row (the node assigns one).
        /// </summary>
        public Guid? UUID { get; set; }

        [Display(Name = "Package Name")]
        public string Name { get; set; }

        public string Version { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Package kind / platform")]
        public LocalSoftwarePackageType LocalSoftwarePackageType { get; set; }

        [Display(Name = "Language of this package")]
        public LanguageMapTypeEnum Language { get; set; }

        /// <summary>Mark a superseded package so GetLatestVersion skips it.</summary>
        [Display(Name = "This Package is Obsolete")]
        public bool Obsolete { get; set; }

        /// <summary>The software package (.zip).</summary>
        public IFormFile File { get; set; }
    }

    /// <summary>
    /// Result of a module-package ingest: the upserted catalog row plus the stored artifact's
    /// bookkeeping (storage key, SHA-256, length) so the caller can verify the checksum.
    /// </summary>
    public class ModulePackageIngestResultViewModel
    {
        public Module Module { get; set; }
        public PackageArtifact Artifact { get; set; }

        /// <summary>
        /// The module's xAPI activity link (ROADMAP 15). A module WITHOUT one is downloadable but
        /// cannot launch -- statement initialization resolves the activity through this row -- so
        /// callers can report the catalog write and the launch linkage separately rather than
        /// implying a launchable module when only the bytes landed.
        /// </summary>
        public ModuleLinkedObject Link { get; set; }

        /// <summary>
        /// Why the ingest is incomplete, or null when everything landed. Set only when
        /// <see cref="Link"/> is null.
        ///
        /// <para>
        /// T10. The contract above was already honoured by the Portal caller and IGNORED by the API
        /// caller, which returned a flat 200 whether or not the module could launch. The null Link
        /// was present in the serialized body, but nothing said what it meant, so an unlaunchable
        /// module was indistinguishable from a good one to anyone not already looking for it.
        /// Carrying the reason on the result makes the partial success self-describing to both
        /// callers rather than relying on each one to remember to check.
        /// </para>
        /// </summary>
        public string StatusMessage { get; set; }
    }

    /// <summary>
    /// Result of a client-software ingest: the upserted catalog row plus the stored artifact's
    /// bookkeeping (storage key, SHA-256, length) so the caller can verify the checksum.
    /// </summary>
    public class SoftwarePackageIngestResultViewModel
    {
        public LocalSoftwarePackage LocalSoftwarePackage { get; set; }
        public PackageArtifact Artifact { get; set; }
    }
}
