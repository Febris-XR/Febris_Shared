// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Febris.ModelLibrary.Models.TicketModels;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for the single shared refresh-token rotation seam (RefreshTokenRotationExtensions) -- the
    /// I-04 consolidation of the rotation branch the three device-token authorization classes (shared
    /// License, shared Hardware, EndUser-island Hardware) previously duplicated. These pin the B-06
    /// guarantee at the seam itself: after rotation, the OLD token reloaded from the cache is revoked
    /// and IsActive == false, and the NEW token is persisted and active. Because the logic now lives in
    /// FebrisSharedServices (testable) rather than the three untestable authorization classes, this is
    /// the regression test the B-06 fix had to defer.
    /// </summary>
    public class RefreshTokenRotationExtensionsTests
    {
        // A Moq IDistributedCache backed by an in-memory dictionary, so SetRecord/GetRecord actually
        // round-trip per key (mirrors the round-trip test in DistributedCacheExtensionsTests).
        private static IDistributedCache InMemoryCache(Dictionary<string, byte[]> store)
        {
            var cache = new Mock<IDistributedCache>();
            cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                    (key, value, _, __) => store[key] = value)
                .Returns(Task.CompletedTask);
            cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string key, CancellationToken _) =>
                    store.TryGetValue(key, out var v) ? v : null);
            return cache.Object;
        }

        [Fact]
        public async Task RevokeAndReplaceAsync_RevokesOldToken_SoReloadedOldIsNotActive()
        {
            var store = new Dictionary<string, byte[]>();
            var cache = InMemoryCache(store);
            const string prefix = "FebrisHardwareToken-";

            var oldToken = new RefreshHardwareToken
            {
                Token = "old-tok",
                Expires = DateTime.UtcNow.AddDays(7),
                Revoked = null,
                LastAuthToken = "old-jwt",
            };
            var newToken = new RefreshHardwareToken
            {
                Token = "new-tok",
                Expires = DateTime.UtcNow.AddDays(8),
                Revoked = null,
                LastAuthToken = "new-jwt",
            };
            // Old token starts out active in the cache (what the refresh path would have loaded).
            await cache.SetRecord(prefix + oldToken.Token, oldToken, TimeSpan.FromDays(8), null);

            await cache.RevokeAndReplaceAsync(oldToken, newToken, prefix, "203.0.113.7", TimeSpan.FromDays(8));

            var reloadedOld = await cache.GetRecord<RefreshHardwareToken>(prefix + "old-tok");
            reloadedOld.Should().NotBeNull();
            reloadedOld.Revoked.Should().NotBeNull("the rotated-out token must be marked revoked");
            reloadedOld.RevokedByIp.Should().Be("203.0.113.7");
            reloadedOld.ReplacedByToken.Should().Be("new-tok");
            reloadedOld.IsActive.Should().BeFalse("a rotated-out token must not be active (B-06)");
        }

        [Fact]
        public async Task RevokeAndReplaceAsync_PersistsNewToken_ActiveAndRoundTripped()
        {
            var store = new Dictionary<string, byte[]>();
            var cache = InMemoryCache(store);
            const string prefix = "FebrisLicenseToken-";

            var oldToken = new RefreshLicenseToken { Token = "old", Expires = DateTime.UtcNow.AddDays(7) };
            var newToken = new RefreshLicenseToken
            {
                Token = "new",
                Expires = DateTime.UtcNow.AddDays(8),
                LastAuthToken = "jwt",
            };

            await cache.RevokeAndReplaceAsync(oldToken, newToken, prefix, "ip", TimeSpan.FromDays(8));

            var reloadedNew = await cache.GetRecord<RefreshLicenseToken>(prefix + "new");
            reloadedNew.Should().NotBeNull("the new token must be persisted under its own key");
            reloadedNew.IsActive.Should().BeTrue();
            reloadedNew.LastAuthToken.Should().Be("jwt", "the persisted record must round-trip");
        }

        [Fact]
        public async Task RevokeAndReplaceAsync_PersistsBothTokens_UnderPrefixedKeys()
        {
            var store = new Dictionary<string, byte[]>();
            var cache = InMemoryCache(store);
            const string prefix = "p-";

            var oldToken = new RefreshHardwareToken { Token = "A", Expires = DateTime.UtcNow.AddDays(7) };
            var newToken = new RefreshHardwareToken { Token = "B", Expires = DateTime.UtcNow.AddDays(8) };

            await cache.RevokeAndReplaceAsync(oldToken, newToken, prefix, "ip", TimeSpan.FromDays(8));

            store.Should().ContainKey("p-A");
            store.Should().ContainKey("p-B");
        }
    }
}
