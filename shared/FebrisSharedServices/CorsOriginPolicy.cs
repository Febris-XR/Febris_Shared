// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;

namespace Febris.SharedServices
{
    /// <summary>
    /// Central CORS origin allow-list for the hosts that have not moved to
    /// <see cref="NodeTransportOptions"/>. A browser origin is allowed when it is localhost or
    /// when its host matches an entry in <see cref="AllowedHosts"/>.
    /// <para>
    /// The allow-list used to be a hardcoded domain check, which trusted exactly one
    /// deployment's frontend and nobody else's. It is now supplied by the host application at
    /// startup and defaults to empty, so an unconfigured host trusts no third-party origin at
    /// all rather than trusting a domain its operator does not own.
    /// </para>
    /// <para>
    /// Matching is delegated to <see cref="NodeTransport.IsOriginAllowed"/> so the rule has one
    /// implementation rather than two that can drift apart. Entry syntax is that method's:
    /// "app.example.com" for an exact host, ".example.com" for the domain and any subdomain.
    /// Because the policy reflects a specific origin and never "*", it remains valid to pair
    /// with AllowCredentials(), which browsers reject alongside a wildcard origin.
    /// </para>
    /// </summary>
    public static class CorsOriginPolicy
    {
        /// <summary>
        /// Hostnames permitted cross-origin. Set once at startup by the host application from its
        /// own configuration. Empty by default: only localhost is allowed until it is configured.
        /// </summary>
        public static string[] AllowedHosts { get; set; } = Array.Empty<string>();

        /// <summary>Origin-allowed predicate for ASP.NET Core CorsPolicyBuilder.SetIsOriginAllowed.</summary>
        public static bool IsFebrisOrigin(string origin)
        {
            return NodeTransport.IsOriginAllowed(origin, AllowedHosts);
        }
    }
}
