// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using Febris.ModelLibrary.Models.UserModels;

namespace Febris.ModelLibrary.ApiContracts
{
    /// <summary>
    /// SSO IdP binding plus its SCIM tokens, for the AdminPortal edit view (served by the SSO API).
    /// The binding's OidcClientSecretEncrypted is replaced by a sentinel server-side so the encrypted
    /// secret never crosses the API boundary.
    /// </summary>
    public class InstitutionIdentityProviderDetail
    {
        public InstitutionIdentityProvider Binding { get; set; }
        public List<InstitutionScimToken> Tokens { get; set; } = new List<InstitutionScimToken>();
    }

    /// <summary>
    /// Upsert request for an IdP binding. The plaintext OIDC client secret travels separately from
    /// the binding so the encrypted form is never trusted from the caller; the SSO re-derives it.
    /// </summary>
    public class InstitutionIdentityProviderSaveRequest
    {
        public InstitutionIdentityProvider Binding { get; set; }
        public string OidcClientSecretPlaintext { get; set; }
    }

    /// <summary>Issue a SCIM token for an institution.</summary>
    public class RotateScimTokenRequest
    {
        public Guid InstitutionUuid { get; set; }
        public string Label { get; set; }
    }
}
