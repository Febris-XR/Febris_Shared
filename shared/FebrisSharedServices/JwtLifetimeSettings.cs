// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using Microsoft.Extensions.Configuration;

namespace Febris.SharedServices
{
    /// <summary>
    /// Token lifetimes for device authentication, read from configuration instead of hardcoded.
    ///
    /// <para>
    /// WHY (audit T9). The device refresh token lived for EIGHT DAYS while the access token lived
    /// fifteen minutes, and the refresh token was only rotated inside the last 24 hours of that
    /// life. Every refresh before then handed back the SAME token, so a stolen refresh token was
    /// usable for roughly a week and nothing noticed. Owner ruling 2026-08-10: eight days is
    /// disproportionate for a short, internally-scoped process, so the refresh token is now EIGHT
    /// HOURS and is rotated on every refresh.
    /// </para>
    ///
    /// <para>
    /// A device can always re-authenticate from scratch with its <c>PhysicalLicense</c>, so a short
    /// refresh window costs at most one extra round-trip. It does not strand a headset.
    /// </para>
    ///
    /// <para>
    /// <c>JwtSettings:ExpiryTimeInSeconds</c> is NOT a new key. It already existed in the API's
    /// appsettings, set to 900 -- exactly the fifteen minutes the code hardcoded -- and NOTHING read
    /// it. It was a knob wired to nothing: an operator who changed it got no effect. It is now the
    /// real source for the access-token lifetime, so the existing config surface starts working
    /// rather than being replaced by an invented one.
    /// </para>
    ///
    /// <para>
    /// Values are clamped rather than trusted. A zero, a negative or an unparseable entry falls back
    /// to the default instead of minting an already-expired token or one that never expires.
    /// </para>
    /// </summary>
    public static class JwtLifetimeSettings
    {
        public const string SectionName = "JwtSettings";

        /// <summary>Fifteen minutes, matching the value the API's appsettings already carried.</summary>
        public static readonly TimeSpan DefaultAccessTokenLifetime = TimeSpan.FromMinutes(15);

        /// <summary>Eight hours -- a shift or a training session, per the 2026-08-10 ruling.</summary>
        public static readonly TimeSpan DefaultRefreshTokenLifetime = TimeSpan.FromHours(8);

        /// <summary>
        /// How long a refresh-token RECORD outlives the token itself in the cache.
        ///
        /// <para>
        /// It must outlive it. Rotation writes the old token back marked <c>Revoked</c>, and that
        /// record is the only evidence that a presented token was already rotated out. If the record
        /// evaporated the moment the token expired, a replayed token would read as merely unknown
        /// rather than as revoked.
        /// </para>
        /// </summary>
        public static readonly TimeSpan RevokedRecordGrace = TimeSpan.FromHours(1);

        /// <summary>Access-token lifetime, from <c>JwtSettings:ExpiryTimeInSeconds</c>.</summary>
        public static TimeSpan AccessTokenLifetime(IConfiguration configuration)
        {
            return Read(configuration, "ExpiryTimeInSeconds", DefaultAccessTokenLifetime, TimeSpan.FromSeconds);
        }

        /// <summary>Refresh-token lifetime, from <c>JwtSettings:RefreshTokenHours</c>.</summary>
        public static TimeSpan RefreshTokenLifetime(IConfiguration configuration)
        {
            return Read(configuration, "RefreshTokenHours", DefaultRefreshTokenLifetime, TimeSpan.FromHours);
        }

        /// <summary>
        /// Cache TTL for a refresh-token record: its lifetime plus the grace window above, so a
        /// rotated-out token stays detectable for a while after it would have expired anyway.
        /// </summary>
        public static TimeSpan RefreshTokenCacheTtl(IConfiguration configuration)
        {
            return RefreshTokenLifetime(configuration) + RevokedRecordGrace;
        }

        private static TimeSpan Read(
            IConfiguration configuration,
            string key,
            TimeSpan fallback,
            Func<double, TimeSpan> unit)
        {
            if (configuration == null)
            {
                return fallback;
            }

            string raw = configuration.GetSection(SectionName)[key];
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallback;
            }

            if (!double.TryParse(raw, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double value))
            {
                FebrisLog.Error(new FormatException("Unparseable value"),
                    SectionName + ":" + key + " is not a number ('" + raw + "'); using the default instead.");
                return fallback;
            }

            if (value <= 0)
            {
                FebrisLog.Error(new ArgumentOutOfRangeException(key),
                    SectionName + ":" + key + " must be greater than zero (was " + raw + "); using the default instead.");
                return fallback;
            }

            return unit(value);
        }
    }
}
