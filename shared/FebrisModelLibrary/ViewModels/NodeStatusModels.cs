// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Node health site (sub-slice 2): everything the Portal status page shows a
    /// node operator -- overall + per-component health, the node's local identity
    /// (<c>NodeIdentity</c>), the node software version, the installed client-software packages,
    /// artifact-store disk usage, and the hub-federation gate state. Deliberately built from
    /// PLAIN types (strings/longs/dates) so the model library carries no framework health-check
    /// dependency; <c>NodeStatusLogic</c> does the mapping. Contains NO secrets: no connection
    /// strings, no storage paths, no hub credentials.
    /// </summary>
    public class NodeStatusViewModel
    {
        /// <summary>Aggregate health verdict ("Healthy" / "Degraded" / "Unhealthy").</summary>
        public string OverallStatus { get; set; }

        /// <summary>One entry per registered health check, mapped from the health report.</summary>
        public List<NodeStatusComponentViewModel> Components { get; set; } = new List<NodeStatusComponentViewModel>();

        /// <summary>The node's human-readable deployment name (null on an unprovisioned store).</summary>
        public string NodeName { get; set; }

        /// <summary>The node's stable institution identity (null on an unprovisioned store).</summary>
        public Guid? InstitutionUUID { get; set; }

        /// <summary>The node software's assembly informational version.</summary>
        public string NodeVersion { get; set; }

        /// <summary>Latest installed client-software package per type (empty store =&gt; empty list).</summary>
        public List<NodeStatusPackageViewModel> InstalledPackages { get; set; } = new List<NodeStatusPackageViewModel>();

        /// <summary>Artifact-store disk usage (graceful "n/a" for non-filesystem backends).</summary>
        public NodeStorageUsageViewModel StorageUsage { get; set; } = new NodeStorageUsageViewModel();

        /// <summary>The ONE hub-federation gate's state. The boolean only -- never endpoints or keys.</summary>
        public bool HubFederationEnabled { get; set; }

        /// <summary>When this snapshot was taken (UTC).</summary>
        public DateTime GeneratedAtUtc { get; set; }
    }

    /// <summary>One health-checked component on the status page (see <see cref="NodeStatusViewModel"/>).</summary>
    public class NodeStatusComponentViewModel
    {
        /// <summary>The registered check name (for example "database-data", "storage").</summary>
        public string Name { get; set; }

        /// <summary>The component verdict ("Healthy" / "Degraded" / "Unhealthy").</summary>
        public string Status { get; set; }

        /// <summary>The check-authored, secret-free description.</summary>
        public string Description { get; set; }

        /// <summary>How long the check took, in milliseconds.</summary>
        public long DurationMs { get; set; }
    }

    /// <summary>
    /// One installed client-software package on the status page: the latest active catalog row of
    /// its type joined to its artifact bookkeeping (checksum) when the bytes were store-ingested.
    /// </summary>
    public class NodeStatusPackageViewModel
    {
        /// <summary>The package kind's display string (PC launcher, Mobile Server, ...).</summary>
        public string PackageType { get; set; }

        /// <summary>Catalog package name.</summary>
        public string Name { get; set; }

        /// <summary>Catalog package version string.</summary>
        public string Version { get; set; }

        /// <summary>SHA-256 of the stored bytes (null when no artifact row exists for the
        /// conventional storage key -- e.g. a catalog row that predates store ingest).</summary>
        public string Sha256 { get; set; }

        /// <summary>When the catalog row was created (the upload date).</summary>
        public DateTime UploadedUtc { get; set; }
    }

    /// <summary>
    /// Artifact-store disk usage. <see cref="HasUsage"/> is false -- the page's graceful "n/a"
    /// path -- whenever the numbers cannot be read honestly: a non-filesystem backend, a blank or
    /// missing base path, or a failed volume query.
    /// </summary>
    public class NodeStorageUsageViewModel
    {
        /// <summary>True only when the byte counts below are real measurements.</summary>
        public bool HasUsage { get; set; }

        /// <summary>The storage backend kind ("FileSystem" / "S3").</summary>
        public string ProviderKind { get; set; }

        /// <summary>Total size of the volume hosting the store's base path.</summary>
        public long TotalBytes { get; set; }

        /// <summary>Bytes in use on that volume.</summary>
        public long UsedBytes { get; set; }

        /// <summary>Bytes still available to the store on that volume.</summary>
        public long AvailableBytes { get; set; }
    }
}
