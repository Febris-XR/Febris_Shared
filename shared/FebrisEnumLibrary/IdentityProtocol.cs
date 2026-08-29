// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.ComponentModel.DataAnnotations;

namespace Febris.EnumLibrary
{
    /// <summary>
    /// Enterprise Tier 1: which federated-identity protocol an
    /// <c>InstitutionIdentityProvider</c> binding uses. SAML 2.0 is the
    /// universal F500 standard (mandatory for v1); OIDC is preferred by
    /// modern IdPs (Okta, Microsoft Entra ID, Auth0, Google Workspace,
    /// Ping). Both ship in v1 -- they share the same per-Institution
    /// binding entity and AdminPortal config surface, only the auth
    /// library differs.
    /// <para>
    /// Numeric values keep gaps so additional protocols (WS-Federation,
    /// legacy SAML 1.1, OAuth-only without OIDC discovery) can slot in
    /// without renumbering. Default <c>0</c> is intentionally unset --
    /// every persisted binding must pick a real protocol.
    /// </para>
    /// </summary>
    public enum IdentityProtocol
    {
        [Display(Name = "Unset")] Unset = 0,
        [Display(Name = "SAML 2.0")] SamlV2 = 100,
        [Display(Name = "OpenID Connect")] Oidc = 200
    }
}
