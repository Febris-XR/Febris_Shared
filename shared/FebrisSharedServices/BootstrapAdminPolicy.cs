// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
namespace Febris.SharedServices
{
    /// <summary>
    /// Pure policy for resolving the first-boot bootstrap admin identity (audit B-03). Keeps the
    /// security-relevant fail-closed decision out of the DI-bound seed so it can be unit-tested:
    /// production must never invent an admin, while dev/staging may fall back to a local default.
    /// </summary>
    public static class BootstrapAdminPolicy
    {
        /// <summary>
        /// Returns the configured bootstrap admin email when set (trimmed). Otherwise returns
        /// <paramref name="devFallback"/> in dev/staging, or null in production (fail closed -- never
        /// invent a production admin).
        /// </summary>
        public static string ResolveEmail(string configuredEmail, bool devOrStaging, string devFallback)
        {
            if (!string.IsNullOrWhiteSpace(configuredEmail)) { return configuredEmail.Trim(); }
            return devOrStaging ? devFallback : null;
        }
    }
}
