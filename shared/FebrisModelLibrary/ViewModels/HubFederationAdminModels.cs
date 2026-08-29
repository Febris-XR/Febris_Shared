// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Read model for the portal's Hub Federation admin page: the SAVED settings row
    /// (or the not-yet-saved defaults) plus the EFFECTIVE gate state the resolver currently
    /// serves. The license key never appears here in full -- only
    /// <see cref="LicenseKeyMasked"/> (last four characters), because the page's key field is
    /// write-only.
    /// </summary>
    public class HubFederationSettingsViewModel
    {
        /// <summary>True when a HubFederationConfig row exists (the operator saved at least once)
        /// -- i.e. the DATABASE governs the gate; false while the legacy configuration resolution
        /// still governs.</summary>
        [Display(Name = "Settings stored on this node")]
        public bool HasStoredSettings { get; set; }

        /// <summary>The stored (or default-off) master switch.</summary>
        [Display(Name = "Federation enabled")]
        public bool Enabled { get; set; }

        /// <summary>The stored hub data API base URL.</summary>
        [Display(Name = "Hub data API")]
        public string DataApi { get; set; }

        /// <summary>The stored hub authentication API base URL.</summary>
        [Display(Name = "Hub authentication API")]
        public string AuthenticationApi { get; set; }

        /// <summary>Masked display form of the stored license key ("****abcd"), or null when no
        /// key is stored. NEVER the full key.</summary>
        [Display(Name = "License key")]
        public string LicenseKeyMasked { get; set; }

        /// <summary>True when a license key is stored (drives the "leave blank to keep" hint).</summary>
        public bool HasLicenseKey { get; set; }

        /// <summary>UTC moment of the last save; null when never saved.</summary>
        [Display(Name = "Last saved (UTC)")]
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>The gate state the resolver is CURRENTLY serving (post placeholder scrub,
        /// post DB/config precedence) -- what the 27 remote paths actually see.</summary>
        [Display(Name = "Gate currently open")]
        public bool EffectiveEnabled { get; set; }
    }

    /// <summary>
    /// Form input for the Hub Federation admin page's Save. The license key field is WRITE-ONLY:
    /// blank means "keep the stored key", a value replaces it, and
    /// <see cref="ClearLicenseKey"/> removes it (the page never round-trips the stored key).
    /// </summary>
    public class HubFederationSettingsInputModel
    {
        /// <summary>Master switch.</summary>
        [Display(Name = "Enable hub federation")]
        public bool Enabled { get; set; }

        /// <summary>Hub data API base URL.</summary>
        [Display(Name = "Hub data API")]
        public string DataApi { get; set; }

        /// <summary>Hub authentication API base URL.</summary>
        [Display(Name = "Hub authentication API")]
        public string AuthenticationApi { get; set; }

        /// <summary>Replacement license key; blank keeps the stored one.</summary>
        [Display(Name = "License key (leave blank to keep the stored key)")]
        public string LicenseKey { get; set; }

        /// <summary>True removes the stored key regardless of <see cref="LicenseKey"/>.</summary>
        [Display(Name = "Remove the stored license key")]
        public bool ClearLicenseKey { get; set; }
    }

    /// <summary>
    /// Result of the admin page's Test Connection action: the gate-aware hub reachability probe
    /// run against the settings the resolver currently serves (i.e. the SAVED settings once the
    /// page has been saved). Plain-typed (no framework health dependency in the model library),
    /// mirroring <see cref="NodeComponentStatusViewModel"/>.
    /// </summary>
    public class HubProbeResultViewModel
    {
        /// <summary>"Healthy" / "Degraded" / "Unhealthy" (HealthStatus name).</summary>
        public string Status { get; set; }

        /// <summary>Human description ("hub reachable (HTTP 401)", "hub federation disabled", ...).
        /// Secret-free by construction -- the probe reports exception TYPES only.</summary>
        public string Description { get; set; }

        /// <summary>UTC moment the probe ran.</summary>
        public DateTime ProbedAtUtc { get; set; }
    }

    /// <summary>
    /// Per-domain outcome of one hub-pull sync pass. Counts describe
    /// ADDITIVE-AND-REFRESH semantics: hub-authored rows insert or refresh matching local rows
    /// by natural key; local-only rows are NEVER deleted.
    /// </summary>
    public class HubSyncDomainResultViewModel
    {
        /// <summary>Domain label ("Verbs", "Objects", "Versions", "Modules", "Module links").</summary>
        public string Domain { get; set; }

        /// <summary>Rows newly created locally.</summary>
        public int Added { get; set; }

        /// <summary>Existing local rows refreshed from the hub-authored copy.</summary>
        public int Updated { get; set; }

        /// <summary>Hub rows that could not be applied (per-row failures; see <see cref="Error"/>
        /// for a whole-domain failure).</summary>
        public int Failed { get; set; }

        /// <summary>Non-null when the whole domain failed (fetch threw / hub unreachable):
        /// exception type + message. Domains are isolated -- one failing never aborts the rest.</summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Summary of one "Sync now" run: per-domain counts plus the gate short-circuit flag. A
    /// closed gate produces <see cref="SkippedGateClosed"/> = true and NO domain entries -- the
    /// same quiet no-op discipline every gated remote path follows.
    /// </summary>
    public class HubSyncSummaryViewModel
    {
        /// <summary>True when the gate was closed and nothing was attempted.</summary>
        public bool SkippedGateClosed { get; set; }

        /// <summary>UTC moment the run started.</summary>
        public DateTime StartedAtUtc { get; set; }

        /// <summary>Per-domain outcomes, in run order.</summary>
        public List<HubSyncDomainResultViewModel> Domains { get; set; } = new List<HubSyncDomainResultViewModel>();
    }
}
