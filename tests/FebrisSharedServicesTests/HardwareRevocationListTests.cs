// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Threading;
using System.Threading.Tasks;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Audit item <c>A-02 Stage 2</c>: immediate revocation of device tokens already in the wild.
    ///
    /// <para>
    /// These use a REAL in-memory distributed cache rather than a mocked
    /// <see cref="IHardwareRevocationList"/>. That is deliberate. The C-03 tests mocked
    /// <c>IActorQueries</c> to RETURN an actor and therefore never exercised the unprovisioned case
    /// that was the entire subject of the finding, and the suite stayed green while the product was
    /// broken. Mocking the revocation list here would repeat that exactly: the tests would prove the
    /// mock works.
    /// </para>
    /// </summary>
    public class HardwareRevocationListTests
    {
        /// <summary>The real cache type the node registers, backed by memory instead of Redis.</summary>
        private sealed class InMemoryHardwareCache : IDistributedHardwareCache
        {
            private readonly MemoryDistributedCache _inner =
                new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

            public byte[] Get(string key) => _inner.Get(key);
            public Task<byte[]> GetAsync(string key, CancellationToken token = default) => _inner.GetAsync(key, token);
            public void Refresh(string key) => _inner.Refresh(key);
            public Task RefreshAsync(string key, CancellationToken token = default) => _inner.RefreshAsync(key, token);
            public void Remove(string key) => _inner.Remove(key);
            public Task RemoveAsync(string key, CancellationToken token = default) => _inner.RemoveAsync(key, token);
            public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _inner.Set(key, value, options);
            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
                => _inner.SetAsync(key, value, options, token);
        }

        /// <summary>A cache whose every operation throws, to prove the fail-open contract.</summary>
        private sealed class BrokenCache : IDistributedHardwareCache
        {
            private static Exception Boom() => new InvalidOperationException("redis is down");
            public byte[] Get(string key) => throw Boom();
            public Task<byte[]> GetAsync(string key, CancellationToken token = default) => throw Boom();
            public void Refresh(string key) => throw Boom();
            public Task RefreshAsync(string key, CancellationToken token = default) => throw Boom();
            public void Remove(string key) => throw Boom();
            public Task RemoveAsync(string key, CancellationToken token = default) => throw Boom();
            public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => throw Boom();
            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => throw Boom();
        }

        private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);

        [Fact]
        public async Task AnUnrevokedDevice_IsNotRevoked()
        {
            HardwareRevocationList list = new HardwareRevocationList(new InMemoryHardwareCache());
            (await list.IsRevokedAsync(Guid.NewGuid())).Should().BeFalse();
        }

        [Fact]
        public async Task RevokingADevice_MakesItRevokedImmediately()
        {
            // The whole point of the change. Locking a device used to leave an already-issued token
            // working until it expired, because the per-request check reads the signed claim.
            InMemoryHardwareCache cache = new InMemoryHardwareCache();
            HardwareRevocationList list = new HardwareRevocationList(cache);
            Guid device = Guid.NewGuid();

            await list.RevokeAsync(device, TokenLifetime);

            (await list.IsRevokedAsync(device)).Should().BeTrue();
        }

        [Fact]
        public async Task RevokingOneDevice_DoesNotAffectAnother()
        {
            HardwareRevocationList list = new HardwareRevocationList(new InMemoryHardwareCache());
            Guid revoked = Guid.NewGuid();
            Guid untouched = Guid.NewGuid();

            await list.RevokeAsync(revoked, TokenLifetime);

            (await list.IsRevokedAsync(revoked)).Should().BeTrue();
            (await list.IsRevokedAsync(untouched)).Should().BeFalse("revocation must be per device, not global");
        }

        [Fact]
        public async Task TheEntryExpires_SoTheListIsSelfEvicting()
        {
            // Entries are written with a TTL equal to the access-token lifetime. Once that elapses
            // the token is dead anyway, so the entry has no further job. This is what keeps the list
            // bounded with no eviction policy and no cleanup job.
            HardwareRevocationList list = new HardwareRevocationList(new InMemoryHardwareCache());
            Guid device = Guid.NewGuid();

            await list.RevokeAsync(device, TimeSpan.FromMilliseconds(120));
            (await list.IsRevokedAsync(device)).Should().BeTrue();

            await Task.Delay(400);

            (await list.IsRevokedAsync(device)).Should().BeFalse("the entry must retire itself once the token it revokes would have expired");
        }

        [Fact]
        public async Task RevokingIsIdempotent()
        {
            HardwareRevocationList list = new HardwareRevocationList(new InMemoryHardwareCache());
            Guid device = Guid.NewGuid();

            await list.RevokeAsync(device, TokenLifetime);
            await list.RevokeAsync(device, TokenLifetime);

            (await list.IsRevokedAsync(device)).Should().BeTrue();
        }

        [Fact]
        public async Task AnEmptyGuid_IsNeverRevoked_AndRevokingItIsANoOp()
        {
            // A token carrying no usable device identity must not be able to poison the list, and
            // must not accidentally match a revocation entry either.
            HardwareRevocationList list = new HardwareRevocationList(new InMemoryHardwareCache());

            await list.RevokeAsync(Guid.Empty, TokenLifetime);

            (await list.IsRevokedAsync(Guid.Empty)).Should().BeFalse();
        }

        [Fact]
        public async Task WhenTheCacheIsDown_ItFailsOPEN_ratherThanLockingEveryDeviceOut()
        {
            // The single most important behaviour here, and the one most likely to be "hardened"
            // later by someone who has not read why. Failing CLOSED on a Redis outage would refuse
            // EVERY device on the node. Failing open degrades to exactly the behaviour that shipped
            // before this existed: a window bounded by token expiry.
            HardwareRevocationList list = new HardwareRevocationList(new BrokenCache());
            Guid device = Guid.NewGuid();

            Func<Task> revoke = async () => await list.RevokeAsync(device, TokenLifetime);
            await revoke.Should().NotThrowAsync("a cache failure must not prevent the lock being saved");

            (await list.IsRevokedAsync(device)).Should().BeFalse(
                "an unreachable cache must not lock every device out of the node");
        }

        [Fact]
        public async Task ANullCache_IsTolerated()
        {
            // The legacy self-newing constructors leave this dependency null.
            HardwareRevocationList list = new HardwareRevocationList(null);
            Guid device = Guid.NewGuid();

            Func<Task> revoke = async () => await list.RevokeAsync(device, TokenLifetime);
            await revoke.Should().NotThrowAsync();
            (await list.IsRevokedAsync(device)).Should().BeFalse();
        }
    }
}
