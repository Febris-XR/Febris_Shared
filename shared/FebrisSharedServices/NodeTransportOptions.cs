// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;

namespace Febris.SharedServices
{
    /// <summary>
    /// Operator-configurable transport security for a self-hosted node (bound from the "<see cref="SectionName"/>"
    /// appsettings section). The platform previously hardcoded HSTS/HTTPS/CORS to Febris's own febr.is domain
    /// (see <see cref="CorsOriginPolicy"/>), which a self-host operator cannot use for their own frontend and
    /// TLS-termination setup. Safe defaults preserve the prior production posture; a missing section keeps them.
    /// </summary>
    public class NodeTransportOptions
    {
        /// <summary>The configuration section these options bind from.</summary>
        public const string SectionName = "Transport";

        /// <summary>HTTP Strict-Transport-Security policy (applied in non-Development environments).</summary>
        public HstsSettings Hsts { get; set; } = new HstsSettings();

        /// <summary>Whether the app itself redirects HTTP to HTTPS. Default off: self-host nodes usually
        /// terminate TLS at a reverse proxy, where an app-level redirect would loop.</summary>
        public bool HttpsRedirection { get; set; } = false;

        /// <summary>Cross-origin (CORS) policy.</summary>
        public CorsSettings Cors { get; set; } = new CorsSettings();

        /// <summary>Static security response headers.</summary>
        public SecurityHeaderSettings SecurityHeaders { get; set; } = new SecurityHeaderSettings();
    }

    /// <summary>HSTS policy.</summary>
    public class HstsSettings
    {
        /// <summary>Whether to emit Strict-Transport-Security (non-Development only). Default on.</summary>
        public bool Enabled { get; set; } = true;
        /// <summary>max-age, in days.</summary>
        public int MaxAgeDays { get; set; } = 365;
        /// <summary>Whether to include the includeSubDomains directive.</summary>
        public bool IncludeSubdomains { get; set; } = true;
        /// <summary>Whether to include the preload directive (only set this once you have submitted to the preload list).</summary>
        public bool Preload { get; set; } = false;
    }

    /// <summary>CORS policy for a node.</summary>
    public class CorsSettings
    {
        /// <summary>
        /// Hostnames permitted cross-origin, e.g. "app.example.com" (exact host) or ".example.com" (the domain
        /// and any subdomain). Empty (the default) means only same-origin + localhost -- no third-party origin
        /// is trusted, unlike the old febr.is hardcoding. Operators list their own frontend host(s).
        /// </summary>
        public string[] AllowedHosts { get; set; } = Array.Empty<string>();

        /// <summary>Whether AllowCredentials is set. Valid because the policy reflects a specific origin, never "*".</summary>
        public bool AllowCredentials { get; set; } = true;
    }

    /// <summary>Static security header toggles.</summary>
    public class SecurityHeaderSettings
    {
        /// <summary>Emit X-Content-Type-Options: nosniff.</summary>
        public bool XContentTypeOptions { get; set; } = true;
        /// <summary>Emit X-XSS-Protection: 1; mode=block.</summary>
        public bool XXssProtection { get; set; } = true;
        /// <summary>
        /// X-Frame-Options clickjacking policy: "SameOrigin" (default -- the app may frame itself, cross-origin
        /// framing blocked), "Deny" (no framing at all), or "Off" (omit the header). Unrecognized values
        /// fail safe to SameOrigin so a typo never silently drops the protection.
        /// </summary>
        public string XFrameOptions { get; set; } = "SameOrigin";
    }

    /// <summary>
    /// The node's operator-configured CORS origin predicate (replaces the febr.is-hardcoded
    /// <see cref="CorsOriginPolicy.IsFebrisOrigin"/> on the self-hostable node hosts). Used as
    /// <c>b.SetIsOriginAllowed(o =&gt; NodeTransport.IsOriginAllowed(o, options.Cors.AllowedHosts))</c>.
    /// </summary>
    public static class NodeTransport
    {
        /// <summary>True when the request origin is localhost/loopback (dev) or its host matches an entry in
        /// <paramref name="allowedHosts"/> (exact host, or a leading-dot entry that also matches subdomains).</summary>
        public static bool IsOriginAllowed(string origin, string[] allowedHosts)
        {
            if (string.IsNullOrWhiteSpace(origin)) { return false; }
            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri uri)) { return false; }

            string host = uri.Host;

            // Local development hosts are always allowed: a victim's browser cannot send from an
            // attacker-controlled localhost origin.
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || host.Equals("127.0.0.1", StringComparison.Ordinal))
            {
                return true;
            }

            if (allowedHosts == null) { return false; }

            foreach (string entry in allowedHosts)
            {
                if (string.IsNullOrWhiteSpace(entry)) { continue; }
                string e = entry.Trim();

                if (e.StartsWith(".", StringComparison.Ordinal))
                {
                    // ".example.com" matches the bare domain AND any subdomain. The leading dot prevents
                    // look-alikes: "evilexample.com" does not end with ".example.com".
                    string bare = e.Substring(1);
                    if (host.Equals(bare, StringComparison.OrdinalIgnoreCase)
                        || host.EndsWith(e, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else if (host.Equals(e, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
