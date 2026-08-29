// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;

namespace Febris.SharedServices
{
    /// <summary>
    /// Builds <see cref="ForwardedHeadersOptions"/> from configuration so an operator can declare
    /// their own reverse proxy, instead of the framework's loopback-only default.
    ///
    /// <para>
    /// WHY THIS MATTERS. Four things on the node read the client IP, and exactly ONE of them acts
    /// on it:
    /// </para>
    /// <list type="bullet">
    /// <item><c>AnalyticsLogic:2271,:2359</c> record it on every request.</item>
    /// <item><c>ModuleDownloadAnalyticsLogic:726</c> and <c>ModuleUsageAnalyticsLogic:739</c> read
    /// <c>X-Real-IP</c>, falling back to the connection address.</item>
    /// <item><c>HardwareKeyAuthorization:133,:167</c> stamps it on the refresh token. RECORDED
    /// ONLY -- there is no comparison anywhere, so this is audit metadata rather than a binding.
    /// An earlier version of this comment called it a security control. It is not.</item>
    /// <item>The RATE LIMITER is the one that acts on it, deciding which bucket a request lands in.
    /// Wrong value means either a trivial bypass or every caller sharing one bucket.</item>
    /// </list>
    /// <para>
    /// With the loopback-only default and a proxy on any other address, all of them see the PROXY's
    /// address rather than the caller's: analytics flatten to a single value, and the rate limiter
    /// locks everyone out together after five requests.
    /// </para>
    ///
    /// <para>
    /// CONFIGURATION, under <c>ForwardedHeaders</c>:
    /// </para>
    /// <list type="bullet">
    /// <item><c>Enabled</c> (default true) turns the whole thing off if an operator terminates TLS
    /// on the host and wants nothing trusted.</item>
    /// <item><c>KnownNetworks</c> is a list of CIDR ranges, for example a Kubernetes pod CIDR like
    /// <c>10.42.0.0/16</c>. This is usually the right answer in a cluster, where the ingress pod's
    /// address is assigned dynamically and cannot be pinned.</item>
    /// <item><c>KnownProxies</c> is a list of literal addresses, for a fixed single proxy.</item>
    /// <item><c>ForwardLimit</c> (default 1) is the number of proxy hops to walk. A cluster with a
    /// load balancer IN FRONT of the ingress is TWO hops, and leaving this at 1 silently attributes
    /// traffic to the ingress rather than the caller.</item>
    /// <item><c>TrustAllProxies</c> (default false) clears both known lists, which makes ASP.NET
    /// accept the forwarded chain from any immediate peer. Safe ONLY where the application is
    /// unreachable except through the ingress, such as a cluster where nothing can route to the pod
    /// directly. It is NOT safe wherever the application also listens on a reachable interface,
    /// because any caller there can forge the whole chain.
    /// <para>
    /// Note the bundled compose stack does NOT qualify: node-api also publishes 8081, so it is
    /// reachable without passing through Caddy. That bind is loopback-only by default since the
    /// H-56 fix, but an operator can widen it to the LAN with NODE_API_HTTP_BIND, so compose
    /// declares KnownNetworks (the pinned subnet) rather than trusting all peers.
    /// </para></item>
    /// </list>
    ///
    /// <para>
    /// DEFAULT BEHAVIOUR IS UNCHANGED. With no <c>ForwardedHeaders</c> section present, this
    /// produces the framework default (loopback only), so adding this cannot alter an existing
    /// deployment until its operator opts in.
    /// </para>
    /// </summary>
    public static class ForwardedHeadersConfiguration
    {
        public const string SectionName = "ForwardedHeaders";

        /// <summary>
        /// Build the options. Returns null when the operator has explicitly disabled the feature,
        /// which the caller should treat as "do not call UseForwardedHeaders at all".
        /// </summary>
        public static ForwardedHeadersOptions Build(IConfiguration configuration)
        {
            IConfigurationSection section = configuration?.GetSection(SectionName);

            if (section != null && section.Exists() && section.GetValue("Enabled", true) == false)
            {
                return null;
            }

            ForwardedHeadersOptions options = new ForwardedHeadersOptions
            {
                // XForwardedProto is what corrects Request.Scheme behind a TLS-terminating proxy.
                // Without it the auth cookie (SecurePolicy=Always) is never emitted, HSTS is
                // skipped, and login cannot work at all.
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };

            if (section == null || !section.Exists())
            {
                return options;
            }

            int forwardLimit = section.GetValue("ForwardLimit", 1);
            options.ForwardLimit = forwardLimit <= 0 ? (int?)null : forwardLimit;

            if (section.GetValue("TrustAllProxies", false))
            {
                // ASP.NET treats empty known-lists as "accept from anyone". See the type remarks
                // for when this is and is not safe.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                return options;
            }

            List<string> networks = ReadList(section, "KnownNetworks");
            List<string> proxies = ReadList(section, "KnownProxies");

            // Only clear the loopback defaults once the operator has actually supplied something,
            // so a malformed or empty section cannot silently strip trust from a working setup.
            if (networks.Count > 0 || proxies.Count > 0)
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            }

            foreach (string cidr in networks)
            {
                Microsoft.AspNetCore.HttpOverrides.IPNetwork parsed = ParseNetwork(cidr);
                if (parsed != null)
                {
                    options.KnownNetworks.Add(parsed);
                }
                else
                {
                    FebrisLog.Error(new FormatException("Unparseable value"),
                        "ForwardedHeaders:KnownNetworks entry '" + cidr + "' is not valid CIDR and was ignored");
                }
            }

            foreach (string address in proxies)
            {
                if (IPAddress.TryParse(address, out IPAddress parsed))
                {
                    options.KnownProxies.Add(parsed);
                }
                else
                {
                    FebrisLog.Error(new FormatException("Unparseable value"),
                        "ForwardedHeaders:KnownProxies entry '" + address + "' is not a valid IP address and was ignored");
                }
            }

            return options;
        }

        private static List<string> ReadList(IConfigurationSection section, string key)
        {
            List<string> values = new List<string>();
            foreach (IConfigurationSection child in section.GetSection(key).GetChildren())
            {
                if (!string.IsNullOrWhiteSpace(child.Value))
                {
                    values.Add(child.Value.Trim());
                }
            }
            return values;
        }

        /// <summary>
        /// CIDR to an ASP.NET <c>IPNetwork</c>. Null when the value cannot be parsed.
        /// Fully qualified: .NET 8 added System.Net.IPNetwork, so the bare name is ambiguous.
        /// </summary>
        private static Microsoft.AspNetCore.HttpOverrides.IPNetwork ParseNetwork(string cidr)
        {
            if (string.IsNullOrWhiteSpace(cidr))
            {
                return null;
            }

            string[] parts = cidr.Split('/');
            if (parts.Length != 2)
            {
                return null;
            }

            if (!IPAddress.TryParse(parts[0], out IPAddress address))
            {
                return null;
            }

            if (!int.TryParse(parts[1], out int prefixLength))
            {
                return null;
            }

            int maxPrefix = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? 128 : 32;
            if (prefixLength < 0 || prefixLength > maxPrefix)
            {
                return null;
            }

            return new Microsoft.AspNetCore.HttpOverrides.IPNetwork(address, prefixLength);
        }
    }
}
