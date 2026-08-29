// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

namespace Febris.SharedServices.Storage
{
    /// <summary>
    /// File-system overhaul (Phase 2, data structures only): one named storage area, expressed as a
    /// logical key prefix. This replaces a single hardcoded <c>StaticDetails.*FileSystemPath</c> holder
    /// with a backend-neutral declaration. A <see cref="KeyPrefix"/> is a forward-slash path relative to
    /// the deployment storage root (the same key shape <see cref="IStorageProvider"/> consumes), never an
    /// absolute path, drive letter, or OS separator. Phase 2 init walks a host's manifest and calls
    /// <see cref="IStorageProvider.EnsureAreaAsync"/> once per area.
    /// <para>
    /// These are definitions only. Nothing here is wired into FileInitalizer or any Startup yet (that is a
    /// later slice).
    /// </para>
    /// </summary>
    public sealed class StorageArea
    {
        /// <summary>Stable, human-readable area name (for diagnostics, config keys, and manifests).</summary>
        public string Name { get; }

        /// <summary>
        /// Logical, forward-slash key prefix for everything stored in this area, relative to the
        /// deployment storage root (for example "media/images", "statements/json", "logs/api"). No leading
        /// slash, no trailing slash, no backend assumptions.
        /// </summary>
        public string KeyPrefix { get; }

        /// <summary>
        /// True when a deployment may turn this area off (an optional/feature area, for example Video or
        /// Marketplace). False for always-on storage a host cannot function without (modules, statements,
        /// images, its own logs).
        /// </summary>
        public bool Optional { get; }

        public StorageArea(string name, string keyPrefix, bool optional = false)
        {
            Name = name;
            KeyPrefix = keyPrefix;
            Optional = optional;
        }
    }

    /// <summary>
    /// The central registry of every storage area the platform knows about. Each field documents the
    /// legacy <c>StaticDetails.*FileSystemPath</c> it replaces and the relative layout it came from, so the
    /// Phase 3 call-site migration can map old absolute paths to these logical key prefixes one-to-one.
    /// <para>
    /// Prefix conventions: all lowercase, forward-slash, no separators baked into the segment names. The
    /// legacy tree mixed casing ("Images", "Logos", "SplitVideos") and back-slashes per build config. The
    /// key model normalizes that -- the provider maps a key back to the OS path.
    /// </para>
    /// Optional vs always-on: areas a deployment can run without are marked Optional=true (Video,
    /// Recordings, and the central catalog areas Marketplace / Publications / EmailCampaign /
    /// LocalSoftwarePackage / Badges / DeveloperLogos). Storage a host needs to function (Images, Modules,
    /// Statements, JsonStatements, VoidStatements, and the per-host logs/api + logs/portal) is Optional=false.
    /// </summary>
    public static class StorageAreas
    {
        // ---- Media: images ----------------------------------------------------------------------

        /// <summary>media/Images (StaticDetails.ImageFileSystemPath). Always-on: the shared image root.</summary>
        public static readonly StorageArea Images = new StorageArea("Images", "media/images");

        /// <summary>media/Images/Logos (StaticDetails.LogoFileSystemPath). Org/tenant logos.</summary>
        public static readonly StorageArea Logos = new StorageArea("Logos", "media/images/logos");

        /// <summary>media/Images/ProfessionalImages (StaticDetails.ProfessionalFileSystemPath). User profile pictures.</summary>
        public static readonly StorageArea Professional = new StorageArea("Professional", "media/images/professional");

        /// <summary>
        /// media/Images/DeveloperLogos (StaticDetails.ContentDeveloperLogoFileSystemPath). Central content-developer
        /// logos. Optional: only central catalog hosts use it.
        /// </summary>
        public static readonly StorageArea DeveloperLogos = new StorageArea("DeveloperLogos", "media/images/developerlogos", optional: true);

        /// <summary>
        /// media/images/badges. Retained for existing deployments only. The microcredential feature that
        /// produced badge art was retired on 2026-08-28, so nothing writes this area any more. Removing it
        /// is a storage-provisioning change and is tracked separately.
        /// </summary>
        public static readonly StorageArea Badges = new StorageArea("Badges", "media/images/badges", optional: true);

        // ---- Media: video -----------------------------------------------------------------------

        /// <summary>
        /// media/video (StaticDetails.VideoFileSystemPath). Optional: a deployment without video lessons does not
        /// need it.
        /// </summary>
        public static readonly StorageArea Video = new StorageArea("Video", "media/video", optional: true);

        /// <summary>
        /// media/video/SplitVideos (StaticDetails.SplitVideoFileSystemPath). Scratch area for chunked-upload parts
        /// before merge. Optional: tied to the video feature.
        /// </summary>
        public static readonly StorageArea SplitVideo = new StorageArea("SplitVideo", "media/video/split", optional: true);

        /// <summary>
        /// media/video/recordings (StaticDetails.RecordingsFileSystemPath). Session recordings. Optional: tied to
        /// the recording feature.
        /// </summary>
        public static readonly StorageArea Recordings = new StorageArea("Recordings", "media/video/recordings", optional: true);

        // ---- Modules ----------------------------------------------------------------------------

        /// <summary>modules (StaticDetails.ModuleFileSystemPath). Always-on: course/module packages.</summary>
        public static readonly StorageArea Modules = new StorageArea("Modules", "modules");

        // ---- Statements -------------------------------------------------------------------------

        /// <summary>statements (StaticDetails.StatementFileSystemPath). Always-on: the statement file root.</summary>
        public static readonly StorageArea Statements = new StorageArea("Statements", "statements");

        /// <summary>statements/JSONstatements (StaticDetails.JSONStatementFileSystemPath). Always-on: xAPI JSON statements.</summary>
        public static readonly StorageArea JsonStatements = new StorageArea("JsonStatements", "statements/json");

        /// <summary>statements/voidstatements (StaticDetails.VoidStatementFileSystemPath). Always-on: voided statements.</summary>
        public static readonly StorageArea VoidStatements = new StorageArea("VoidStatements", "statements/void");

        // ---- Central catalog (marketplace / publications / packages) ----------------------------

        /// <summary>
        /// MarketplaceListings (StaticDetails.MarketplaceListingPath). Per-listing media lives under
        /// {prefix}/{listingId}/screenshot and {prefix}/{listingId}/video (the old per-listing Screenshot/ and
        /// Video/ subfolders). Optional: central marketplace only.
        /// </summary>
        public static readonly StorageArea Marketplace = new StorageArea("Marketplace", "marketplace/listings", optional: true);

        /// <summary>
        /// Publications (StaticDetails.PublicationPath). Per-publication media under {prefix}/{publicationId}/images
        /// and {prefix}/{publicationId}/videos (the old per-listing Images/ and Videos/ subfolders). Optional:
        /// central publications only.
        /// </summary>
        public static readonly StorageArea Publications = new StorageArea("Publications", "publications", optional: true);

        /// <summary>
        /// EmailCampaign (StaticDetails.EmailCampaignPath). Campaign assets under {prefix}/images (the old Images/
        /// subfolder). Optional: central marketing only.
        /// </summary>
        public static readonly StorageArea EmailCampaign = new StorageArea("EmailCampaign", "emailcampaign", optional: true);

        /// <summary>
        /// LocalSoftwarePackage (StaticDetails.LocalSoftwarePackage). Downloadable software packages. Optional:
        /// central distribution only.
        /// </summary>
        public static readonly StorageArea LocalSoftwarePackage = new StorageArea("LocalSoftwarePackage", "localsoftwarepackage", optional: true);

        // ---- Logs (per host) --------------------------------------------------------------------

        /// <summary>logs/api (StaticDetails.APILogFileSystemPath). Always-on for any host running the API.</summary>
        public static readonly StorageArea ApiLogs = new StorageArea("ApiLogs", "logs/api");

        /// <summary>logs/portal (StaticDetails.PortalLogFileSystemPath). Always-on for any host running a portal.</summary>
        public static readonly StorageArea PortalLogs = new StorageArea("PortalLogs", "logs/portal");

        /// <summary>
        /// logs/adminportal (StaticDetails.AdminPortalLogFileSystemPath). Central admin-portal logs ONLY. Optional,
        /// and deliberately absent from the EndUser manifest: an EndUser deployment must never materialize an
        /// adminportal area (that is the exact bug Phase 2 fixes).
        /// </summary>
        public static readonly StorageArea AdminPortalLogs = new StorageArea("AdminPortalLogs", "logs/adminportal", optional: true);
    }

    /// <summary>
    /// The set of storage areas a single host declares it uses. Phase 2 init walks a host's manifest and
    /// calls <see cref="IStorageProvider.EnsureAreaAsync"/> once per area, so no host ever creates another
    /// host's areas. Order is preserved (parents before children) so a filesystem provider that does not
    /// already mkdir-p still produces a sane tree.
    /// </summary>
    public sealed class StorageManifest
    {
        /// <summary>The areas this host owns, in declaration order.</summary>
        public IReadOnlyList<StorageArea> Areas { get; }

        public StorageManifest(params StorageArea[] areas)
            : this((IEnumerable<StorageArea>)areas)
        {
        }

        public StorageManifest(IEnumerable<StorageArea> areas)
        {
            Areas = (areas ?? Enumerable.Empty<StorageArea>()).ToList();
        }
    }

    /// <summary>
    /// The per-host manifests. Definitions only -- no Startup or FileInitalizer wiring (that is a later
    /// slice).
    /// </summary>
    public static class StorageManifests
    {
        /// <summary>
        /// Central hosts: the FULL area set, matching the legacy FileInitalizer FileList exactly (every media
        /// area, the whole central catalog, and all four log dirs INCLUDING logs/adminportal).
        /// </summary>
        public static readonly StorageManifest Central = new StorageManifest(
            // Media: images
            StorageAreas.Images,
            StorageAreas.Logos,
            StorageAreas.Professional,
            StorageAreas.DeveloperLogos,
            StorageAreas.Badges,
            // Media: video
            StorageAreas.Video,
            StorageAreas.SplitVideo,
            StorageAreas.Recordings,
            // Modules
            StorageAreas.Modules,
            // Statements
            StorageAreas.Statements,
            StorageAreas.JsonStatements,
            StorageAreas.VoidStatements,
            // Central catalog
            StorageAreas.Marketplace,
            StorageAreas.Publications,
            StorageAreas.EmailCampaign,
            StorageAreas.LocalSoftwarePackage,
            // Logs (all hosts, including adminportal)
            StorageAreas.ApiLogs,
            StorageAreas.PortalLogs,
            StorageAreas.AdminPortalLogs);

        /// <summary>
        /// EndUser hosts: the SCOPED subset. This fixes the "EndUser creates adminportal" problem -- the EndUser
        /// deployment must only ever see its own tenant storage (auth-island / uncontrolled-deployment boundary).
        /// The included set is grounded in what the EndUser code actually references, plus the adminportal-free
        /// intent of the 2024 commented-out EndUser handler
        /// (enduser/FebrisEndUserPortal/LocalUtility/FileServerHandler.cs).
        /// <para>
        /// Included: media (images + logos/professional, video/splitvideo/recordings), modules,
        /// statements (json + void), and its own logs/api + logs/portal. EXPLICITLY EXCLUDED (the EndUser does
        /// not use these locally): logs/adminportal, content-developer logos, the marketplace listing tree
        /// (served via the remote core API, not a local path), and -- since the ROADMAP 17 reachability sweep
        /// deleted WidgetController.BadgeLoader / PublicationImageLoader / CampaignEmailMessageImageLoader,
        /// whose serving was the only reason they were ever here -- badges, publications, and email-campaign
        /// assets.
        /// </para>
        /// <para>
        /// Downloadable software packages moved from the excluded list INTO
        /// the EndUser manifest -- the node is now the distribution point for its own client software
        /// (mobile Server APK, Companion APK, PC launcher installer, integration SDKs), ingested through
        /// IStorageProvider into the localsoftwarepackage area instead of proxied from central.
        /// </para>
        /// </summary>
        public static readonly StorageManifest EndUser = new StorageManifest(
            // Media: images (no central DeveloperLogos, and no Badges since ROADMAP 17)
            StorageAreas.Images,
            StorageAreas.Logos,
            StorageAreas.Professional,
            // Media: video
            StorageAreas.Video,
            StorageAreas.SplitVideo,
            StorageAreas.Recordings,
            // Modules
            StorageAreas.Modules,
            // Statements
            StorageAreas.Statements,
            StorageAreas.JsonStatements,
            StorageAreas.VoidStatements,
            // Client-software distribution: the node's own software-package store
            StorageAreas.LocalSoftwarePackage,
            // Logs: its own api + portal ONLY (NO adminportal)
            StorageAreas.ApiLogs,
            StorageAreas.PortalLogs);
    }
}
