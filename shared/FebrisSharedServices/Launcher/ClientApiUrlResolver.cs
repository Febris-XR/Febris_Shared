// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using System;

namespace Febris.SharedServices.Launcher
{
    /// <summary>
    /// Gives the PC BACKGROUND processes a way to learn the node's API URL (ROADMAP 21).
    ///
    /// <para>
    /// <see cref="LocalHardwareStaticDetails.ApiUrl"/> ships EMPTY by design -- severance requires
    /// that an unconfigured client fail rather than phone home to Febris. It was populated only by
    /// <c>URLSettingUtility.SetURL()</c>, which exists solely in the PC Launcher and the Mobile
    /// Server. <c>LocalHardwareStaticDetails</c> is a STATIC in this shared library, and statics
    /// are per-process, so the separate Topshelf service processes -- the statement uploader and
    /// the module manager -- read an `ApiUrl` that nothing in their process ever assigned. It was
    /// empty for their whole lifetime and every request resolved to a relative "Token/...". Neither
    /// could reach the node at all. No IPC channel carried it either: the shared memory-mapped
    /// files are `creds`, `febrisToken`, `simulationRunningCheck` and `uniqueIdentifier`.
    /// </para>
    ///
    /// <para>
    /// Configuration is the channel chosen because both processes ALREADY build an
    /// <see cref="IConfiguration"/> at startup, the builder already chains
    /// <c>AddEnvironmentVariables()</c> (so a service can be pointed at a node with
    /// <c>ApiUrlPath__DataApi</c> without editing a file), and it matches how the node's own hosts
    /// are configured. The alternatives considered were a per-process copy of
    /// <c>URLSettingUtility</c> reading the Launcher's persisted ConfigModel, and a fifth
    /// memory-mapped file carrying the URL alongside the token.
    /// </para>
    ///
    /// <para>
    /// It still fails CLOSED. An absent or placeholder value leaves <c>ApiUrl</c> empty and returns
    /// false with a usable message, so the caller can refuse to start loudly and legibly. The point
    /// of the severance rule is that a misconfigured client must not silently reach Febris -- not
    /// that it must be undiagnosable.
    /// </para>
    /// </summary>
    public static class ClientApiUrlResolver
    {
        /// <summary>Primary key. Matches the node hosts' own <c>ApiUrlPath</c> section.</summary>
        public const string PrimaryKey = "ApiUrlPath:DataApi";

        /// <summary>Fallback key, for a deployment that only sets the auth endpoint.</summary>
        public const string FallbackKey = "ApiUrlPath:AuthenticationApi";

        /// <summary>
        /// Read the API URL from configuration and apply it to
        /// <see cref="LocalHardwareStaticDetails.ApiUrl"/>. Returns false, with an operator-readable
        /// reason, when no usable value is configured -- in which case ApiUrl is left EMPTY rather
        /// than guessed at.
        /// </summary>
        public static bool TryApply(IConfiguration configuration, out string error)
        {
            error = null;

            if (configuration == null)
            {
                error = "No configuration was supplied, so the node API URL could not be read. "
                      + "Set " + PrimaryKey + " in appsettings.json, or the ApiUrlPath__DataApi environment variable.";
                return false;
            }

            string value = configuration[PrimaryKey];
            if (string.IsNullOrWhiteSpace(value) || IsPlaceholder(value))
            {
                value = configuration[FallbackKey];
            }

            if (string.IsNullOrWhiteSpace(value) || IsPlaceholder(value))
            {
                error = "The node API URL is not configured. Set " + PrimaryKey + " (or " + FallbackKey
                      + ") in appsettings.json next to the executable, or the ApiUrlPath__DataApi "
                      + "environment variable, to this deployment's node -- for example "
                      + "https://node.example.org:5102/api/ . It is deliberately not defaulted: a "
                      + "client that cannot be told which node it belongs to must not guess one.";
                return false;
            }

            // The request builders concatenate directly ("Token/" + method), so a missing trailing
            // separator silently produces ".../apiToken/Refresh".
            LocalHardwareStaticDetails.ApiUrl = value.EndsWith("/", StringComparison.Ordinal)
                ? value
                : value + "/";
            return true;
        }

        /// <summary>
        /// True for an unsubstituted deploy token such as <c>{ApiUrl}</c>. Same conservative shape
        /// as JwtSigningKeyProvider's placeholder detection: exactly one outer brace pair, nothing
        /// nested, no whitespace.
        /// </summary>
        private static bool IsPlaceholder(string value)
        {
            string trimmed = value.Trim();
            if (trimmed.Length < 3 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
            {
                return false;
            }
            for (int i = 1; i < trimmed.Length - 1; i++)
            {
                char c = trimmed[i];
                if (c == '{' || c == '}' || char.IsWhiteSpace(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
