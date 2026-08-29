// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;

namespace Febris.SharedServices
{
    /// <summary>
    /// Pure policy for validating a post-login returnUrl before redirecting, to prevent open redirects
    /// (audit B-01). The framework-bound local-URL check (IUrlHelper.IsLocalUrl) stays in the page,
    /// because it depends on request context. This class owns the testable, security-critical part:
    /// deciding whether an ABSOLUTE returnUrl points at an allowed Febris portal origin. Mirrors the
    /// testable-static-policy shape of <see cref="MfaPolicy"/>.
    /// </summary>
    public static class ReturnUrlPolicy
    {
        /// <summary>
        /// True when <paramref name="returnUrl"/> is an absolute URL whose scheme, host, and port match
        /// one of <paramref name="allowedPortalBaseUrls"/>. Path and query are ignored, so any path on an
        /// allowed portal is accepted. Returns false for null/empty, relative, non-parseable, or
        /// origin-mismatched values. Host comparison is case-insensitive.
        /// </summary>
        public static bool IsAllowedPortalOrigin(string returnUrl, IEnumerable<string> allowedPortalBaseUrls)
        {
            if (string.IsNullOrEmpty(returnUrl)) { return false; }
            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out Uri target)) { return false; }
            foreach (string baseUrl in allowedPortalBaseUrls ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrEmpty(baseUrl)
                    && Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri allowed)
                    && Uri.Compare(target, allowed, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
