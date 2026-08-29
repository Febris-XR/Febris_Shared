// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.UserModels;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels
{
    // AdminPortal SSO-federation view models. Relocated from
    // InstitutionIdentityProviderController per the "models + view models live
    // in FebrisModelLibrary" rule (R1).

    /// <summary>
    /// Index row -- pairs a binding with its resolved Institution name.
    /// </summary>
    public class InstitutionIdentityProviderIndexRow
    {
        public InstitutionIdentityProvider Binding { get; set; }
        public string InstitutionName { get; set; }
    }

    /// <summary>
    /// Edit page model -- binding + plaintext OIDC secret entry field + the token list (for the SCIM
    /// token panel) + the parent Institution (for the page header).
    /// </summary>
    public class InstitutionIdentityProviderEditViewModel
    {
        public InstitutionIdentityProvider Binding { get; set; }

        /// <summary>
        /// Plaintext OIDC client secret entry. Empty = "don't change." Never persisted in this shape
        /// -- the SSO DataProtection-wraps it before insert.
        /// </summary>
        public string OidcClientSecretPlaintext { get; set; }

        public List<InstitutionScimToken> Tokens { get; set; } = new List<InstitutionScimToken>();

        /// <summary>
        /// Resolved Institution (for header display). May be null if the binding's InstitutionUUID
        /// points at a tenant that's been deleted; the view handles that gracefully.
        /// </summary>
        public Institution Institution { get; set; }
    }
}
