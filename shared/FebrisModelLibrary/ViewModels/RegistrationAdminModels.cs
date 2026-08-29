// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// The node's STORED registration policy, flattened for consumption above the data layer.
    ///
    /// <para>
    /// This type exists because of the layering boundary, not despite it. The registration
    /// mode ENUM and <c>IdentityPolicyOptions</c> live in the portal assembly, which the logic
    /// layer cannot reference, and the logic layer must not hand a raw entity upwards. So the
    /// logic layer answers in this neutral shape (mode as a NAME, domains as the flat string the
    /// form edits) and the portal is the only place that turns a name back into an enum.
    /// </para>
    ///
    /// <para>
    /// <see cref="HasStoredSettings"/> is the load-bearing member: false means the operator never
    /// saved, so the configured <c>Identity:Registration</c> section still governs. It is NOT the
    /// same as a failed read, which never produces one of these at all.
    /// </para>
    /// </summary>
    public class StoredRegistrationPolicy
    {
        /// <summary>True when a row exists. False means "never saved", which is a normal state
        /// and hands governance back to configuration.</summary>
        public bool HasStoredSettings { get; set; }

        /// <summary>The stored mode NAME, or null when nothing is stored.</summary>
        public string Mode { get; set; }

        /// <summary>Allowed self-registration domains, comma-separated.</summary>
        public string AllowedEmailDomains { get; set; }

        /// <summary>Whether a newly self-registered account must be approved by an admin.</summary>
        public bool RequireAdminApproval { get; set; }

        /// <summary>Whether an unknown external-IdP user is auto-provisioned on first login.</summary>
        public bool AutoProvisionJit { get; set; }

        /// <summary>Optional UTC expiry after which the stored mode stops governing.</summary>
        public DateTime? OpenUntilUtc { get; set; }

        /// <summary>UTC moment of the last save; null when never saved.</summary>
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>Email of the admin who last saved; null when never saved or not recorded.</summary>
        public string UpdatedByEmail { get; set; }
    }

    /// <summary>
    /// Read model for the portal's Registration admin page: the SAVED policy row (or the
    /// configured values still governing in its absence) plus the EFFECTIVE posture the resolver
    /// currently serves. Mirrors <see cref="HubFederationSettingsViewModel"/>, which is the same
    /// shape for the other node-local operator-owned setting.
    /// <para>
    /// Nothing here is masked. A registration mode is policy, not a credential, so unlike the
    /// federation page there is no write-only field.
    /// </para>
    /// </summary>
    public class RegistrationSettingsViewModel
    {
        /// <summary>True when a NodeRegistrationConfig row exists (the operator saved at least
        /// once) -- i.e. the DATABASE governs; false while the configured
        /// <c>Identity:Registration</c> section still governs.</summary>
        [Display(Name = "Policy stored on this node")]
        public bool HasStoredSettings { get; set; }

        /// <summary>The stored mode name, or the configured mode when nothing is stored.</summary>
        [Display(Name = "Registration mode")]
        public string Mode { get; set; }

        /// <summary>Allowed self-registration domains, comma-separated, as the form edits them.</summary>
        [Display(Name = "Allowed email domains")]
        public string AllowedEmailDomains { get; set; }

        /// <summary>Whether a newly self-registered account must be approved by an admin.</summary>
        [Display(Name = "Require admin approval")]
        public bool RequireAdminApproval { get; set; }

        /// <summary>Whether an unknown external-IdP user is auto-provisioned on first login.</summary>
        [Display(Name = "Auto-provision external logins")]
        public bool AutoProvisionJit { get; set; }

        /// <summary>Optional UTC expiry after which the stored mode reverts to AdminOnly on its
        /// own; null when the mode is open-ended.</summary>
        [Display(Name = "Open until (UTC)")]
        public DateTime? OpenUntilUtc { get; set; }

        /// <summary>True when <see cref="OpenUntilUtc"/> is set and has already passed, so the
        /// page can say that the stored mode is no longer the effective one.</summary>
        public bool OpenWindowExpired { get; set; }

        /// <summary>The mode the resolver is CURRENTLY serving -- after stored-versus-configured
        /// precedence, after the expiry check, and after the unparseable-value fallback. This is
        /// what the register page actually sees, and it is what the screen leads with.</summary>
        [Display(Name = "Effective registration mode")]
        public string EffectiveMode { get; set; }

        /// <summary>True when the effective mode admits unauthenticated self-registration
        /// (Open or DomainAllowlist).</summary>
        [Display(Name = "Self-registration currently open")]
        public bool EffectiveSelfRegistrationEnabled { get; set; }

        /// <summary>The mode configured in <c>Identity:Registration:Mode</c>, shown so an operator
        /// can see what the node falls back to if the stored row is cleared.</summary>
        [Display(Name = "Configured mode (fallback)")]
        public string ConfiguredMode { get; set; }

        /// <summary>
        /// The configured <c>RequireConfirmedEmail</c>, shown READ-ONLY. It is deliberately not
        /// editable here because it is also copied into ASP.NET Identity's
        /// <c>SignIn.RequireConfirmedEmail</c> at startup, so a runtime change would move the
        /// register page without moving sign-in.
        /// </summary>
        [Display(Name = "Require confirmed email (configuration only)")]
        public bool RequireConfirmedEmailConfigured { get; set; }

        /// <summary>UTC moment of the last save; null when never saved.</summary>
        [Display(Name = "Last saved (UTC)")]
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>Email of the admin who last saved; null when never saved or not recorded.</summary>
        [Display(Name = "Last saved by")]
        public string UpdatedByEmail { get; set; }
    }

    /// <summary>
    /// Form input for the Registration admin page's Save. Every field is editable; there is no
    /// write-only member (contrast <see cref="HubFederationSettingsInputModel"/>, whose license
    /// key is a credential).
    /// </summary>
    public class RegistrationSettingsInputModel
    {
        /// <summary>The requested mode NAME. An unrecognized value is rejected by the logic layer
        /// rather than stored, so a bad post cannot park the node on an unparseable policy.</summary>
        [Display(Name = "Registration mode")]
        public string Mode { get; set; }

        /// <summary>Allowed self-registration domains, comma-separated.</summary>
        [Display(Name = "Allowed email domains")]
        public string AllowedEmailDomains { get; set; }

        /// <summary>Whether a newly self-registered account must be approved by an admin.</summary>
        [Display(Name = "Require admin approval")]
        public bool RequireAdminApproval { get; set; }

        /// <summary>Whether an unknown external-IdP user is auto-provisioned on first login.</summary>
        [Display(Name = "Auto-provision external logins")]
        public bool AutoProvisionJit { get; set; }

        /// <summary>
        /// Optional number of hours to keep the selected mode before it reverts to AdminOnly by
        /// itself. Null or zero means open-ended. Hours rather than an absolute timestamp because
        /// the operator is thinking "for this afternoon", not "until 16:00 UTC", and because a
        /// relative value cannot be posted already-expired by a stale form.
        /// </summary>
        [Display(Name = "Close automatically after (hours)")]
        public int? OpenForHours { get; set; }
    }
}
