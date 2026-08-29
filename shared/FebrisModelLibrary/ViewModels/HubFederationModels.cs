// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// The ONE hub-federation gate covering all hub calls: the node's read-only view of whether a
    /// central hub is configured and, when it is, where it lives. Every remaining
    /// tenant-to-central call path (the license bootstrap in
    /// <c>TokenQueries</c>, the DataApi Remote query classes, and their token-renewal retry hooks)
    /// consults this gate before doing anything remote. When the gate is closed the node behaves
    /// LOCAL-ONLY: no HTTP attempt is made, nothing is logged, and callers receive the same quiet
    /// empty results an unreachable hub already produced.
    /// </summary>
    public interface IHubFederationSettings
    {
        /// <summary>Master switch. Default FALSE: a node with no hub configured never federates.</summary>
        bool Enabled { get; }

        /// <summary>Base URL of the hub's data API (the legacy <c>ApiUrlPath:DataApi</c> value).</summary>
        string DataApi { get; }

        /// <summary>Base URL of the hub's authentication API (the legacy <c>ApiUrlPath:AuthenticationApi</c> value).</summary>
        string AuthenticationApi { get; }

        /// <summary>The scheme-B hub credential (the legacy root <c>LicenseKey</c> value). An
        /// OPT-IN federation credential only -- the node operates fully without one.</summary>
        string LicenseKey { get; }

        /// <summary>True only when the gate is open AND a DataApi endpoint is configured -- the
        /// per-call check for every DataApi Remote query.</summary>
        bool CanReachDataApi { get; }

        /// <summary>True only when the gate is open AND an AuthenticationApi endpoint is
        /// configured -- the per-call check for the token bootstrap/renewal paths.</summary>
        bool CanReachAuthenticationApi { get; }

        /// <summary>True only when the gate is open AND a parseable LicenseKey is configured --
        /// the additional check for the scheme-B license bootstrap.</summary>
        bool HasLicenseKey { get; }
    }

    /// <summary>
    /// Invalidation hook for a CACHING <see cref="IHubFederationSettings"/> implementation (the
    /// DB-first resolver registered by <c>AddFebrisUserNodeDataAccess</c>). The portal's
    /// federation-settings save path calls <see cref="Invalidate"/> so the freshly persisted row
    /// governs the very next gate consultation on this host instead of waiting out the short TTL;
    /// OTHER hosts sharing the tenant DataDb converge within the TTL (the database is the only
    /// cross-process channel). Separate from the read contract so the 27 gate CONSUMERS never see
    /// a mutating member.
    /// </summary>
    public interface IHubFederationSettingsCache
    {
        /// <summary>Drop the cached snapshot; the next consultation re-resolves DB-first.</summary>
        void Invalidate();
    }

    /// <summary>
    /// Config-bound implementation of <see cref="IHubFederationSettings"/>. Bound from the
    /// <c>"HubFederation"</c> configuration section; see <see cref="Resolve(IConfiguration)"/> for
    /// the resolution (and legacy back-compat) rules. Registered as a DI singleton by
    /// <c>AddFebrisUserNodeDataAccess</c>; legacy self-newing query classes resolve the same
    /// gate state from the configuration they already hold via <see cref="Resolve(IConfiguration)"/>
    /// (a pure function -- no static state is introduced).
    /// </summary>
    public class HubFederationSettings : IHubFederationSettings
    {
        /// <summary>The configuration section this gate binds from.</summary>
        public const string SectionName = "HubFederation";

        /// <inheritdoc />
        public bool Enabled { get; set; } = false;

        /// <inheritdoc />
        public string DataApi { get; set; }

        /// <inheritdoc />
        public string AuthenticationApi { get; set; }

        /// <inheritdoc />
        public string LicenseKey { get; set; }

        /// <inheritdoc />
        public bool CanReachDataApi => Enabled && !string.IsNullOrWhiteSpace(DataApi);

        /// <inheritdoc />
        public bool CanReachAuthenticationApi => Enabled && !string.IsNullOrWhiteSpace(AuthenticationApi);

        /// <inheritdoc />
        public bool HasLicenseKey => Enabled && Guid.TryParse(LicenseKey, out Guid parsed) && parsed != Guid.Empty;

        /// <summary>A permanently-closed gate -- the answer for a null/absent configuration.</summary>
        public static HubFederationSettings Disabled()
        {
            return new HubFederationSettings();
        }

        /// <summary>
        /// Resolve the gate from configuration. Rules, in order:
        /// <list type="number">
        /// <item>No configuration at all (null) -- gate closed. A config-less construction (unit
        /// tests, tooling) must never NRE.</item>
        /// <item>A <c>"HubFederation"</c> section exists -- it GOVERNS: <c>Enabled</c> (default
        /// false) plus the endpoint/credential values are bound from the section only. This is the
        /// forward-looking shape: one section, one switch.</item>
        /// <item>No <c>"HubFederation"</c> section, but the LEGACY keys are present
        /// (<c>ApiUrlPath:DataApi</c>/<c>ApiUrlPath:AuthenticationApi</c> plus a root
        /// <c>LicenseKey</c>) -- treated as an enabled hub so existing deployments keep federating
        /// unchanged without a config migration. Legacy endpoints WITHOUT a LicenseKey (or a key
        /// without endpoints) do not open the gate: the scheme-B credential is what made the old
        /// coupling live.</item>
        /// </list>
        /// Pure function over the supplied configuration -- callers that hold a different config
        /// source (DI options vs the legacy passed-back static) get identical answers for
        /// identical values.
        /// </summary>
        public static HubFederationSettings Resolve(IConfiguration configuration)
        {
            return Resolve(configuration, null);
        }

        /// <summary>
        /// <see cref="Resolve(IConfiguration)"/> with a deploy-placeholder guard (MED-6 family):
        /// any endpoint/credential value for which <paramref name="isUnresolvedPlaceholder"/>
        /// returns true (an unsubstituted <c>{Token}</c> the deploy-time substitution never
        /// replaced) is treated as ABSENT before the enablement rules run, so a
        /// placeholder-riddled config resolves the gate DISABLED instead of open-against-garbage:
        /// <list type="bullet">
        /// <item>legacy shape -- placeholder endpoints/key no longer count as "configured", so
        /// <c>Enabled</c> stays false;</item>
        /// <item>section shape -- placeholder values are blanked, and an explicit
        /// <c>Enabled=true</c> is demoted to false when NO real endpoint survives (a half-real
        /// config keeps its surviving endpoint and lets the health check report the gap).</item>
        /// </list>
        /// The predicate is a parameter (rather than a hard reference to the SharedServices
        /// helper) because the model library sits BELOW SharedServices in the project graph;
        /// callers pass <c>JwtSigningKeyProvider.IsUnsubstitutedTemplate</c> -- the same helper
        /// the MED-6 startup validator uses. Null predicate = the historical behavior, unchanged.
        /// Still a pure function over its arguments.
        /// </summary>
        public static HubFederationSettings Resolve(IConfiguration configuration, Func<string, bool> isUnresolvedPlaceholder)
        {
            if (configuration == null)
            {
                return Disabled();
            }

            IConfigurationSection section = configuration.GetSection(SectionName);
            if (section.Exists())
            {
                HubFederationSettings bound = section.Get<HubFederationSettings>() ?? Disabled();
                bound.DataApi = ScrubPlaceholder(bound.DataApi, isUnresolvedPlaceholder);
                bound.AuthenticationApi = ScrubPlaceholder(bound.AuthenticationApi, isUnresolvedPlaceholder);
                bound.LicenseKey = ScrubPlaceholder(bound.LicenseKey, isUnresolvedPlaceholder);
                if (bound.Enabled
                    && string.IsNullOrWhiteSpace(bound.DataApi)
                    && string.IsNullOrWhiteSpace(bound.AuthenticationApi))
                {
                    bound.Enabled = false;
                }
                return bound;
            }

            // Legacy back-compat: pre-gate deployments configured ApiUrlPath + LicenseKey.
            string dataApi = ScrubPlaceholder(
                configuration.GetSection("ApiUrlPath").GetValue<string>("DataApi"), isUnresolvedPlaceholder);
            string authenticationApi = ScrubPlaceholder(
                configuration.GetSection("ApiUrlPath").GetValue<string>("AuthenticationApi"), isUnresolvedPlaceholder);
            string licenseKey = ScrubPlaceholder(
                configuration.GetValue<string>("LicenseKey"), isUnresolvedPlaceholder);

            bool legacyConfigured =
                (!string.IsNullOrWhiteSpace(dataApi) || !string.IsNullOrWhiteSpace(authenticationApi))
                && !string.IsNullOrWhiteSpace(licenseKey);

            return new HubFederationSettings()
            {
                Enabled = legacyConfigured,
                DataApi = dataApi,
                AuthenticationApi = authenticationApi,
                LicenseKey = licenseKey
            };
        }

        /// <summary>Null when the value is an unsubstituted deploy placeholder per the supplied
        /// predicate; the value unchanged otherwise (including when no predicate is supplied).</summary>
        private static string ScrubPlaceholder(string value, Func<string, bool> isUnresolvedPlaceholder)
        {
            if (isUnresolvedPlaceholder != null && isUnresolvedPlaceholder(value))
            {
                return null;
            }
            return value;
        }
    }
}
