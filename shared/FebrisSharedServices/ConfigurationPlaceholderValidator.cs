// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Febris.SharedServices
{
    /// <summary>
    /// MED-6: detects unresolved deploy-time configuration placeholders -- values still left as a
    /// literal token like <c>"{DataDBConnectionString}"</c> because the k8s ConfigMap/Secret (or
    /// other deploy-time substitution) did not inject a real value. Without this, an un-injected
    /// key crashes the app cryptically downstream (or, worse, runs against the literal
    /// <c>"{...}"</c> string).
    ///
    /// Detection reuses <see cref="JwtSigningKeyProvider.IsUnsubstitutedTemplate"/> so it matches
    /// the existing JWT-secret check exactly: the WHOLE value must be a single <c>{Token}</c> with
    /// no nested braces or whitespace. This deliberately does NOT flag values that merely CONTAIN a
    /// brace token (for example a Serilog <c>"log-{Date}.json"</c> pathFormat), which are not deploy
    /// placeholders.
    /// </summary>
    public static class ConfigurationPlaceholderValidator
    {
        /// <summary>
        /// Config key for the opt-in fail-fast switch. When set to <c>true</c>, <see cref="Validate"/>
        /// throws on any unresolved placeholder instead of just returning the offending keys. Default
        /// (absent/false) is log-only, so wiring this in never changes startup behavior until an
        /// operator opts in.
        /// </summary>
        public const string FailFastKey = "ConfigValidation:FailFastOnUnresolvedPlaceholders";

        /// <summary>
        /// Returns the configuration keys whose value is an unsubstituted <c>{Placeholder}</c>.
        /// </summary>
        public static List<string> FindUnresolvedPlaceholders(IConfiguration configuration)
        {
            List<string> unresolved = new List<string>();
            if (configuration == null)
            {
                return unresolved;
            }
            foreach (KeyValuePair<string, string> entry in configuration.AsEnumerable())
            {
                if (JwtSigningKeyProvider.IsUnsubstitutedTemplate(entry.Value))
                {
                    unresolved.Add(entry.Key);
                }
            }
            return unresolved;
        }

        /// <summary>
        /// In non-Development, finds unresolved placeholder values and returns the offending keys for
        /// the caller to log. If <see cref="FailFastKey"/> is set <c>true</c>, throws
        /// <see cref="InvalidOperationException"/> instead (a clear startup failure beats a cryptic
        /// downstream crash). In Development this is a no-op -- dev uses concrete local values -- and
        /// returns an empty list.
        /// </summary>
        /// <summary>
        /// Convenience overload that derives the environment from <c>ASPNETCORE_ENVIRONMENT</c>
        /// (case-insensitive "Development" -> dev). Use from <c>ConfigureServices</c>, where
        /// <c>IWebHostEnvironment</c> is not in scope.
        /// </summary>
        public static List<string> Validate(IConfiguration configuration)
        {
            bool isDevelopment = string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);
            return Validate(configuration, isDevelopment);
        }

        public static List<string> Validate(IConfiguration configuration, bool isDevelopment)
        {
            if (isDevelopment || configuration == null)
            {
                return new List<string>();
            }

            List<string> unresolved = FindUnresolvedPlaceholders(configuration);
            if (unresolved.Count == 0)
            {
                return unresolved;
            }

            string failFastRaw = configuration[FailFastKey];
            bool failFast = bool.TryParse(failFastRaw, out bool parsed) && parsed;
            if (failFast)
            {
                throw new InvalidOperationException(
                    "Unresolved configuration placeholders (no deploy-time value was injected for these keys): " +
                    string.Join(", ", unresolved) +
                    ". Inject the values via the environment / ConfigMap / Secret, or set " +
                    FailFastKey + "=false to downgrade this to a startup warning.");
            }

            return unresolved;
        }
    }
}
