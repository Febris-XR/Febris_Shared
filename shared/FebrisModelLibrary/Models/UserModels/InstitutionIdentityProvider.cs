// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.UserModels
{
    /// <summary>
    /// Enterprise Tier 1: per-<c>Institution</c> binding to the customer's
    /// external identity provider (Okta, Microsoft Entra ID, AD FS,
    /// PingFederate, Google Workspace, Auth0, etc.). When a row exists
    /// for an Institution, the SSO API delegates login for users of
    /// that Institution to the configured IdP via SAML 2.0 or OIDC;
    /// when no row exists, local-password Identity-Framework login is
    /// used (the SMB / schools / internal Febris path).
    /// <para>
    /// v1 = one binding per Institution. Multi-IdP per Institution is
    /// deferred to v1.6 if a deal requires it (e.g. dev + prod IdPs).
    /// </para>
    /// <para>
    /// <b>Where it lives:</b> UserDB, same DbContext as
    /// <c>ApplicationUser</c>. The link to <c>Institution</c> is a UUID
    /// reference (no DB-level FK -- <c>Institution</c> lives on DataDB).
    /// Validation that the UUID resolves is a BLL concern.
    /// </para>
    /// <para>
    /// <b>Secrets at rest:</b> <c>OidcClientSecretEncrypted</c> is
    /// always DataProtection-encrypted before persistence (see
    /// <c>InstitutionIdentityProviderLogic.SaveAsync</c>). The SAML
    /// signing cert is a public cert (no encryption needed) but is
    /// stored normalized so rotation visibility is easy in the AdminPortal.
    /// </para>
    /// </summary>
    public class InstitutionIdentityProvider : BaseModel
    {
        [Required]
        [Display(Name = "Institution")]
        public Guid InstitutionUUID { get; set; }

        [Required]
        [Display(Name = "Protocol")]
        public IdentityProtocol Protocol { get; set; } = IdentityProtocol.Unset;

        // ---- SAML 2.0 fields ----

        [Display(Name = "SAML metadata XML")]
        public string SamlMetadataXml { get; set; }

        [Display(Name = "SAML signing cert")]
        public string SamlSigningCert { get; set; }

        [Display(Name = "SAML entity ID")]
        public string SamlEntityId { get; set; }

        [Display(Name = "SAML SSO URL")]
        public string SamlSsoUrl { get; set; }

        [Display(Name = "SAML SLO URL")]
        public string SamlSloUrl { get; set; }

        [Display(Name = "Allow IdP-initiated launch")]
        public bool SamlAllowIdpInitiated { get; set; } = true;

        // ---- OIDC fields ----

        [Display(Name = "OIDC authority")]
        public string OidcAuthority { get; set; }

        [Display(Name = "OIDC client ID")]
        public string OidcClientId { get; set; }

        [Display(Name = "OIDC client secret (encrypted)")]
        public string OidcClientSecretEncrypted { get; set; }

        [Display(Name = "OIDC scopes")]
        public string OidcScopes { get; set; } = "openid profile email";

        // ---- Shared mapping + behavior ----

        [Display(Name = "Claim mapping (JSON)")]
        public string ClaimMappingJson { get; set; }

        [Display(Name = "Default role on first login")]
        public string DefaultRoleOnJit { get; set; } = "EndUser";

        [Display(Name = "Auto-provision new users on first login (JIT)")]
        public bool AutoProvisionOnFirstLogin { get; set; } = true;

        [Display(Name = "Enforce federated login only")]
        public bool EnforceFederatedLoginOnly { get; set; }

        [Display(Name = "Enabled")]
        public bool Enabled { get; set; } = true;

        // ---- Audit ----

        [Display(Name = "Created by")]
        public Guid? CreatedByUserId { get; set; }

        [Display(Name = "Last modified by")]
        public Guid? LastModifiedByUserId { get; set; }
    }
}
