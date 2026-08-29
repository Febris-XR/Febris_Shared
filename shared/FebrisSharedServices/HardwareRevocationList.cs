// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Febris.SharedServices
{
    /// <summary>
    /// Immediate revocation for device tokens already in the wild. Closes audit item
    /// <c>A-02 Stage 2</c>.
    ///
    /// <para>
    /// THE GAP THIS FILLS. A device token is a JWT carrying the whole <c>Hardware</c> object as a
    /// claim, <c>IsLockedOut</c> included. The middleware validates the signature and deserialises
    /// that claim straight into the request, consulting neither the database nor any cache. Locking
    /// a device is therefore correctly enforced at token ISSUANCE and at REFRESH, both of which
    /// re-read the live row, but a token already issued keeps working until it expires. Measured
    /// lifetime: 15 minutes. So the node's only device-revocation control had a fifteen minute lag
    /// against a device that had already authenticated.
    /// </para>
    ///
    /// <para>
    /// WHY A REVOCATION LIST AND NOT THE OBVIOUS ALTERNATIVES. Deleting the cached refresh ticket
    /// does nothing, because refresh already refuses a locked device by re-reading the row, and the
    /// live token holder never presents the ticket again inside the window. Re-reading the database
    /// on every device request is the option this codebase deliberately rejected as a hot-path cost,
    /// and that objection is sound: this node's own health check measures a database round-trip at
    /// 15-21ms against 2ms for Redis. Shortening the token lifetime trades one lag for a large
    /// increase in refresh traffic, and each refresh is a database read.
    /// </para>
    ///
    /// <para>
    /// SELF-EVICTING BY DESIGN. Entries are written with a TTL equal to the access-token lifetime.
    /// Once that elapses the token is expired anyway, so the entry has no further work to do and
    /// Redis drops it. There is no eviction policy to maintain, no cleanup job, and the list cannot
    /// grow beyond the devices revoked within one token lifetime.
    /// </para>
    ///
    /// <para>
    /// FAILS OPEN, DELIBERATELY. If the cache is unreachable, <see cref="IsRevokedAsync"/> reports
    /// false and the caller proceeds. That degrades to exactly the behaviour that shipped before
    /// this existed, a window bounded by token expiry. Failing CLOSED would mean a Redis outage
    /// locks every device out of the node, which is a far worse failure than the one being fixed.
    /// The trade is stated here so nobody "hardens" it later without weighing that.
    /// </para>
    /// </summary>
    public interface IHardwareRevocationList
    {
        /// <summary>
        /// Mark a device's outstanding tokens as revoked for <paramref name="tokenLifetime"/>.
        /// Safe to call repeatedly. Never throws: a cache failure is logged and swallowed, because
        /// refusing to save a lock because Redis is down would be worse than the lag.
        /// </summary>
        Task RevokeAsync(Guid hardwareUuid, TimeSpan tokenLifetime);

        /// <summary>
        /// True when this device has been revoked and its outstanding tokens must be refused.
        /// Returns FALSE if the cache cannot be reached. See the fail-open note on the interface.
        /// </summary>
        Task<bool> IsRevokedAsync(Guid hardwareUuid);
    }

    /// <inheritdoc />
    public class HardwareRevocationList : IHardwareRevocationList
    {
        /// <summary>
        /// Distinct from <c>HardwareKeyAuthorization</c>'s refresh-token prefix on purpose. These
        /// are different lifetimes and different meanings sharing one Redis instance.
        /// </summary>
        private const string KeyPrefix = "hardware-revoked:";

        private readonly IDistributedHardwareCache _cache;

        public HardwareRevocationList(IDistributedHardwareCache cache)
        {
            _cache = cache;
        }

        private static string KeyFor(Guid hardwareUuid)
        {
            return KeyPrefix + hardwareUuid.ToString("N");
        }

        /// <inheritdoc />
        public async Task RevokeAsync(Guid hardwareUuid, TimeSpan tokenLifetime)
        {
            if (_cache == null || hardwareUuid == Guid.Empty)
            {
                return;
            }

            try
            {
                // The value is irrelevant. Presence of the key is the signal, and the TTL is what
                // makes the entry retire itself once the token it revokes would have expired.
                await _cache.SetStringAsync(
                    KeyFor(hardwareUuid),
                    "1",
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = tokenLifetime
                    });
            }
            catch (Exception ex)
            {
                // Swallowed on purpose. The lock itself is already persisted to the database by the
                // caller, and issuance and refresh both re-read that row, so a failure here costs
                // the immediacy, not the revocation.
                FebrisLog.Error(ex, "HardwareRevocationList.RevokeAsync: cache write failed, revocation falls back to token expiry");
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsRevokedAsync(Guid hardwareUuid)
        {
            if (_cache == null || hardwareUuid == Guid.Empty)
            {
                return false;
            }

            try
            {
                string hit = await _cache.GetStringAsync(KeyFor(hardwareUuid));
                return hit != null;
            }
            catch (Exception ex)
            {
                // FAIL OPEN. See the interface note: a Redis outage must not lock every device out.
                FebrisLog.Error(ex, "HardwareRevocationList.IsRevokedAsync: cache read failed, allowing the request");
                return false;
            }
        }
    }
}
