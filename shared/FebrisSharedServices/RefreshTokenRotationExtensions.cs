// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Threading.Tasks;
using Febris.ModelLibrary.Models.TicketModels;
using Microsoft.Extensions.Caching.Distributed;

namespace Febris.SharedServices
{
    /// <summary>
    /// The ONE shared device/license refresh-token rotation seam.
    ///
    /// The three device-token authorization classes -- shared LicenseKeyAuthorization and shared
    /// HardwareKeyAuthorization (both in FebrisSharedLogicLayer), and the EndUser-island
    /// HardwareKeyAuthorization (FebrisEndUserBLL) -- previously each duplicated the rotation branch
    /// inline. That three-way duplication is audit finding I-04 and is the structural reason the
    /// B-06 / MDM-B4 revoke-on-rotation fix had to be written three separate times (and reached only
    /// one copy when it was first applied). This extension is their single call site.
    ///
    /// It lives in FebrisSharedServices because that assembly is referenced by BOTH the central
    /// shared logic layer AND the EndUser island (the island may share EnumLibrary + ModelLibrary +
    /// SharedServices only), so consolidating here does NOT introduce a central reference into the
    /// island and preserves the EndUser auth-island invariant.
    /// </summary>
    public static class RefreshTokenRotationExtensions
    {
        /// <summary>
        /// Rotates a refresh token: marks the OLD token revoked (Revoked / RevokedByIp /
        /// ReplacedByToken), then persists BOTH the revoked old token and the new token back to the
        /// cache under keyPrefix + Token with the given TTL. Persisting the revoked old token is what
        /// flips its cached IsActive to false so a rotated-out token can no longer be refreshed (the
        /// B-06 fix). Persisting both from one place is what stops the three copies from drifting
        /// again (I-04). Generic over the concrete token type so the full derived record (incl.
        /// LastAuthToken) is serialized, not just the base.
        /// </summary>
        public static async Task RevokeAndReplaceAsync<T>(
            this IDistributedCache cache,
            T oldToken,
            T newToken,
            string keyPrefix,
            string revokedByIp,
            TimeSpan ttl)
            where T : BaseRefreshLicenseToken
        {
            oldToken.Revoked = DateTime.UtcNow;
            oldToken.RevokedByIp = revokedByIp;
            oldToken.ReplacedByToken = newToken.Token;

            // Persist the revoked OLD token first so its IsActive == false is durable, then the new one.
            await cache.SetRecord(keyPrefix + oldToken.Token, oldToken, ttl, null);
            await cache.SetRecord(keyPrefix + newToken.Token, newToken, ttl, null);
        }
    }
}
