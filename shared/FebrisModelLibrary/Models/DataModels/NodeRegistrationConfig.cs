// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// The node's persisted registration policy row -- the operator's runtime answer to
    /// "who may create an account here". Node initialization design 2026-08-18: initialization
    /// itself was never the gap (the seeded bootstrap admin already solves it); the gap was that
    /// <c>Identity:Registration:Mode</c> could only be changed by editing a JSON file and
    /// restarting the host. This row is the turnable version of that policy.
    ///
    /// <para>
    /// Exactly ONE row per deployment (single-row semantics like <see cref="NodeIdentity"/> and
    /// <see cref="HubFederationConfig"/>, and written by the portal admin surface rather than a
    /// provision-time seeder -- absence simply means "the operator never touched the page", in
    /// which case the configured <c>Identity:Registration</c> section keeps governing exactly as
    /// before). NODE-ONLY -- never mapped centrally.
    /// </para>
    ///
    /// <para>
    /// Deliberately NOT a mirror of every leaf under <c>Identity:Registration</c>. It carries only
    /// the leaves that are genuinely re-read on every request, so the admin screen cannot promise
    /// something the running host will not honor. <c>RequireConfirmedEmail</c> is the one left out:
    /// it is ALSO copied into <c>IdentityOptions.SignIn.RequireConfirmedEmail</c> at startup, so a
    /// runtime change would move the register page's behavior while sign-in kept the boot value.
    /// It stays configuration-only, and the screen says so.
    /// </para>
    /// </summary>
    public class NodeRegistrationConfig : BaseModel
    {
        /// <summary>
        /// The stored registration mode, held as the ENUM NAME ("AdminOnly", "Invite", "Open",
        /// "DomainAllowlist") rather than an ordinal.
        ///
        /// <para>
        /// Text, not an int, for two reasons. The enum lives in the portal assembly and the model
        /// library cannot reference it, and a name survives someone reordering the enum whereas a
        /// stored ordinal would silently change meaning -- an ordinal shift that turned AdminOnly
        /// into Open is precisely the failure this whole feature exists to prevent. A value that
        /// does not parse resolves AdminOnly (see the portal's registration resolver).
        /// </para>
        /// </summary>
        [Display(Name = "Registration mode")]
        public string Mode { get; set; }

        /// <summary>
        /// Email domains permitted to self-register under <c>DomainAllowlist</c>, comma-separated
        /// ("acme.com,beta.org"). Stored flat rather than as a Postgres <c>text[]</c> so the column
        /// maps on every provider the tests and the deployment use, and because the form field is
        /// itself free text. Entries may be written with or without a leading "@" -- the existing
        /// allowlist comparison normalizes both. Null or empty means no domain is admitted, which
        /// makes DomainAllowlist with an empty list equivalent to closed rather than open.
        /// </summary>
        [Display(Name = "Allowed email domains")]
        public string AllowedEmailDomains { get; set; }

        /// <summary>Whether a newly self-registered account must be approved by an admin before it
        /// is usable. Read per request by the register and external-login flows.</summary>
        [Display(Name = "Require admin approval")]
        public bool RequireAdminApproval { get; set; }

        /// <summary>Whether an unknown user authenticated by an external IdP is auto-provisioned a
        /// local account on first login. False = closed SSO. Read per request by the external-login
        /// flow.</summary>
        [Display(Name = "Auto-provision external logins")]
        public bool AutoProvisionJit { get; set; }

        /// <summary>
        /// Optional UTC expiry for an OPEN window. When set, the stored mode governs only until
        /// this moment and the policy reverts to <c>AdminOnly</c> afterwards WITHOUT anyone having
        /// to remember to close it.
        ///
        /// <para>
        /// This is the mitigation the initialization design attached to open registration: the
        /// objection to "open it, then toggle it off" was never that operators are careless, it was
        /// that the window has no floor and its failure is silent. An expiry gives the window a
        /// floor. Null means the mode is open-ended, which is the right shape for
        /// <c>AdminOnly</c> and <c>Invite</c> and a deliberate choice for <c>Open</c>.
        /// </para>
        /// </summary>
        [Display(Name = "Open until (UTC)")]
        public DateTime? OpenUntilUtc { get; set; }

        /// <summary>UTC moment of the last portal save. App-set (unlike the DB-managed
        /// <see cref="BaseModel.LastUpdateTimeStamp"/>) so the admin page can display it
        /// provider-neutrally, including on the InMemory test store.</summary>
        [Display(Name = "Last updated (UTC)")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Email of the admin who last saved this row, recorded because opening registration on a
        /// node holding learner records is an audit-worthy event and "who opened it" is the first
        /// question anyone asks. Nullable: a row written by a path with no signed-in principal
        /// (tests, future automation) records no actor rather than inventing one.
        /// </summary>
        [Display(Name = "Last updated by")]
        public string UpdatedByEmail { get; set; }
    }
}
