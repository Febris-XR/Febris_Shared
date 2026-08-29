// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// Enterprise Tier 1: result of
    /// <c>InstitutionIdentityProviderLogic.RotateScimTokenAsync</c>.
    /// Carries the plaintext token back to the admin EXACTLY ONCE
    /// (the AdminPortal Edit page renders it then forgets it). The
    /// persisted row stores only the SHA-256 hash + display prefix.
    /// </summary>
    public class ScimTokenIssueResult
    {
        public string Plaintext { get; set; }
        public string TokenPrefix { get; set; }
        public Guid TokenUuid { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
