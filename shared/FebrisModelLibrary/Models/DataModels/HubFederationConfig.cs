// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// The node's persisted hub-federation settings row (hub-pull sync; owner-ratified
    /// design 2026-07-17: the OPERATOR owns federation -- the license key is a marketplace
    /// membership credential, never an operate requirement, and opt-in happens via the node
    /// portal).
    ///
    /// <para>
    /// Exactly ONE row per deployment (single-row semantics like <see cref="NodeIdentity"/>, but
    /// written by the portal admin surface rather than a provision-time seeder -- absence simply
    /// means "the operator never touched the page"). When the row exists it GOVERNS the ONE
    /// hub-federation gate (<c>IHubFederationSettings</c>); when it does not, the legacy
    /// configuration resolution (<c>HubFederation</c> section / <c>ApiUrlPath</c>+<c>LicenseKey</c>
    /// back-compat) keeps governing unchanged. NODE-ONLY -- never mapped centrally.
    /// </para>
    /// </summary>
    public class HubFederationConfig : BaseModel
    {
        /// <summary>Master switch, exactly the semantics of the gate's Enabled: default FALSE,
        /// a node that never opted in never federates.</summary>
        [Display(Name = "Federation enabled")]
        public bool Enabled { get; set; }

        /// <summary>Base URL of the hub's data API.</summary>
        [Display(Name = "Hub data API")]
        public string DataApi { get; set; }

        /// <summary>Base URL of the hub's authentication API.</summary>
        [Display(Name = "Hub authentication API")]
        public string AuthenticationApi { get; set; }

        /// <summary>
        /// The marketplace membership credential, ENCRYPTED AT REST: this column stores the
        /// IDataProtection payload produced by the DAL's dedicated protector purpose
        /// (see <c>HubFederationConfigQueries</c>), never the plaintext key. Nullable -- an
        /// operator may configure endpoints without a key (vocabulary pull needs none).
        /// </summary>
        [Display(Name = "License key (protected)")]
        public string LicenseKey { get; set; }

        /// <summary>UTC moment of the last portal save. App-set (unlike the DB-managed
        /// <see cref="BaseModel.LastUpdateTimeStamp"/>) so the admin page can display it
        /// provider-neutrally, including on the InMemory test store.</summary>
        [Display(Name = "Last updated (UTC)")]
        public DateTime UpdatedAt { get; set; }
    }
}
