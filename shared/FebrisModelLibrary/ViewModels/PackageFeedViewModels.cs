// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Wire shape of a published client-software distribution manifest, as defined by
    /// <c>distribution/schema/manifest.schema.json</c>.
    /// <para>
    /// The feed is an anonymous, read-only, static document (GitHub Releases plus this index): there
    /// is no distribution service to operate, because distribution is read-mostly over immutable
    /// artifacts.
    /// </para>
    /// </summary>
    public class PackageFeedManifest
    {
        /// <summary>
        /// Bumped ONLY on a breaking change to this document's shape. A consumer must refuse a value
        /// it does not know rather than guess: guessing at an unknown schema is how a sync silently
        /// ingests the wrong thing.
        /// </summary>
        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; }

        /// <summary>
        /// When the manifest was produced. Informational ONLY. Never use it to decide which package
        /// is newest, because regenerating a manifest does not mean a new release.
        /// </summary>
        [JsonProperty("generated")]
        public DateTime? Generated { get; set; }

        /// <summary>
        /// Every advertised artifact, including obsolete ones. Order carries no meaning: a consumer
        /// that needs an ordering must sort by <see cref="PackageFeedEntry.VersionCode"/> itself.
        /// </summary>
        [JsonProperty("packages")]
        public List<PackageFeedEntry> Packages { get; set; }
    }

    /// <summary>One published artifact. Field-for-field mappable onto a LocalSoftwarePackage row.</summary>
    public class PackageFeedEntry
    {
        /// <summary>
        /// STABLE release identity, assigned by the publisher and never reused. This is what makes a
        /// re-sync idempotent, because the node upserts its catalog row keyed on it, so ingesting the
        /// same release twice updates one row instead of creating two.
        /// </summary>
        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        /// <summary>Artifact kind by name. Mirrors the <see cref="LocalSoftwarePackageType"/> member.</summary>
        [JsonProperty("kind")]
        public string Kind { get; set; }

        /// <summary>
        /// The <see cref="LocalSoftwarePackageType"/> integer, carried alongside the name so no
        /// consumer maintains a name-to-enum mapping that can drift. Redundant ON PURPOSE, which is
        /// why a disagreement with <see cref="Kind"/> is a fatal manifest error rather than something
        /// to resolve in favour of one side.
        /// </summary>
        [JsonProperty("kindId")]
        public int KindId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Android's monotonic upgrade counter. Required for the Android kinds, meaningless for the
        /// rest. The sync orders by this ASCENDING, because the node resolves "latest" by row
        /// TimeStamp rather than by version.
        /// </summary>
        [JsonProperty("versionCode")]
        public int? VersionCode { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        /// <summary>
        /// Who is expected to fetch this. <c>["human","node"]</c> for the mobile Server, which is the
        /// BOOTSTRAP and must be clickable by a person because a tablet with no Febris app cannot
        /// fetch its own first app. <c>["node"]</c> for the Companion, matching the developer portal
        /// deliberately refusing to serve it to a browser.
        /// </summary>
        [JsonProperty("consumers")]
        public List<string> Consumers { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>Optional <see cref="LanguageMapTypeEnum"/> member name. Defaulted when absent.</summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Android application id. PERMANENT once published, on the same footing as the signing key:
        /// it cannot change without every user uninstalling and losing local data.
        /// </summary>
        [JsonProperty("packageName")]
        public string PackageName { get; set; }

        [JsonProperty("minSdk")]
        public int? MinSdk { get; set; }

        [JsonProperty("targetSdk")]
        public int? TargetSdk { get; set; }

        /// <summary>Mirrors the catalog's own flag so "latest non-obsolete" resolves the same both sides.</summary>
        [JsonProperty("obsolete")]
        public bool Obsolete { get; set; }

        [JsonProperty("artifact")]
        public PackageFeedArtifact Artifact { get; set; }

        [JsonProperty("contains")]
        public List<PackageFeedContent> Contains { get; set; }

        [JsonProperty("releaseNotes")]
        public string ReleaseNotes { get; set; }
    }

    /// <summary>
    /// The downloadable unit. Always a .zip wrapping the payload, because the deployed mobile client
    /// already downloads and writes a .zip, and preserving that envelope means no mobile-side change.
    /// </summary>
    public class PackageFeedArtifact
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("sizeBytes")]
        public long SizeBytes { get; set; }

        /// <summary>
        /// Lowercase hex SHA-256 of the zip as served, matching the casing PackageArtifact.Sha256
        /// records so the two compare directly. MUST be verified before the artifact is committed to
        /// the catalog, or a truncated download becomes a published package.
        /// </summary>
        [JsonProperty("sha256")]
        public string Sha256 { get; set; }
    }

    /// <summary>What is inside the zip, so a consumer can verify the payload and not just its wrapper.</summary>
    public class PackageFeedContent
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("sha256")]
        public string Sha256 { get; set; }

        /// <summary>
        /// SHA-256 of the APK SIGNING CERTIFICATE, not of the file. The only value here that proves
        /// ORIGIN: a public URL plus a public checksum shows the bytes arrived intact and says nothing
        /// about who produced them, since whoever serves the file also serves the checksum.
        /// </summary>
        [JsonProperty("signerSha256")]
        public string SignerSha256 { get; set; }
    }

    /// <summary>What an admin asks the node to pull.</summary>
    public class PackageFeedSyncRequestViewModel
    {
        /// <summary>
        /// Absolute URL of the manifest. Required, and deliberately not defaulted to a compiled
        /// constant: a node operator points at whichever feed they trust, and an air-gapped operator
        /// can point at a file served on their own network.
        /// </summary>
        public string ManifestUrl { get; set; }

        /// <summary>Channel to sync. Defaults to <c>stable</c> when omitted.</summary>
        public string Channel { get; set; }

        /// <summary>
        /// Optional kind filter. Empty means every kind the manifest offers to nodes.
        /// </summary>
        public List<LocalSoftwarePackageType> Kinds { get; set; }

        /// <summary>
        /// Report what WOULD be ingested and change nothing. On by default at the API would be
        /// surprising, so it defaults false, but an operator should run it once before the real thing.
        /// </summary>
        public bool DryRun { get; set; }
    }

    /// <summary>Per-package outcome of a sync. Deliberately granular, so a partial run is legible.</summary>
    public enum PackageFeedSyncOutcome
    {
        /// <summary>Ingested into the catalog and the artifact store.</summary>
        Ingested = 0,

        /// <summary>Already held at this UUID with a matching checksum. Nothing to do.</summary>
        AlreadyCurrent = 1,

        /// <summary>Excluded by the channel filter, the kind filter, the node-consumer rule, or being obsolete.</summary>
        Filtered = 2,

        /// <summary>Would have been ingested. Only produced by a dry run.</summary>
        WouldIngest = 3,

        /// <summary>
        /// REFUSED. Something was wrong enough that ingesting would have been unsafe: a checksum
        /// mismatch, a kind/kindId disagreement, or a UUID already held whose checksum differs from
        /// the one the manifest now advertises.
        /// </summary>
        Refused = 4,

        /// <summary>The download or the store write failed. Nothing was committed.</summary>
        Failed = 5
    }

    /// <summary>One line of the sync report.</summary>
    public class PackageFeedSyncItemViewModel
    {
        public Guid Uuid { get; set; }
        public string Kind { get; set; }
        public string Version { get; set; }
        public PackageFeedSyncOutcome Outcome { get; set; }

        /// <summary>Human-readable reason. Always populated for Refused and Failed.</summary>
        public string Detail { get; set; }
    }

    /// <summary>
    /// Result of one sync run. A run does not fail as a whole because one package did: each package
    /// reports its own outcome, so one bad entry in a feed cannot block every good one.
    /// </summary>
    public class PackageFeedSyncResultViewModel
    {
        public string ManifestUrl { get; set; }
        public string Channel { get; set; }
        public bool DryRun { get; set; }

        /// <summary>Schema version the manifest declared, recorded so a refusal is explainable.</summary>
        public int SchemaVersion { get; set; }

        public int Ingested { get; set; }
        public int AlreadyCurrent { get; set; }
        public int Filtered { get; set; }
        public int Refused { get; set; }
        public int Failed { get; set; }

        public List<PackageFeedSyncItemViewModel> Items { get; set; } = new List<PackageFeedSyncItemViewModel>();
    }
}
