// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="DistributedCacheExtensions"/>.
    ///
    /// <para>
    /// The extensions wrap <see cref="IDistributedCache"/> with a typed serialize / deserialize
    /// layer using <c>System.Text.Json</c>. Tests use a mocked <see cref="IDistributedCache"/>
    /// (Moq) and assert on the JSON payload that gets passed through, plus the
    /// <see cref="DistributedCacheEntryOptions"/> the extension constructs.
    /// </para>
    /// </summary>
    public class DistributedCacheExtensionsTests
    {
        // Sample payload type for round-tripping through the cache extension.
        private class Payload
        {
            public string Name { get; set; }
            public int Score { get; set; }
        }

        // --- SetRecord -----------------------------------------------------------------------

        [Fact]
        public async Task SetRecord_SerializesValueToJsonAndCallsSetStringAsync()
        {
            var cache = new Mock<IDistributedCache>(MockBehavior.Strict);
            string capturedKey = null;
            byte[] capturedValue = null;

            cache.Setup(c => c.SetAsync(It.IsAny<string>(),
                                       It.IsAny<byte[]>(),
                                       It.IsAny<DistributedCacheEntryOptions>(),
                                       It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                    (key, value, _, __) => { capturedKey = key; capturedValue = value; })
                .Returns(Task.CompletedTask);

            var data = new Payload { Name = "Riley", Score = 99 };

            await cache.Object.SetRecord("payload:1", data);

            capturedKey.Should().Be("payload:1");
            capturedValue.Should().NotBeNull();
            var json = Encoding.UTF8.GetString(capturedValue);
            json.Should().Contain("\"Name\":\"Riley\"");
            json.Should().Contain("\"Score\":99");
        }

        [Fact]
        public async Task SetRecord_WithoutExpirationArgs_UsesSixtySecondAbsoluteDefault()
        {
            // Default contract per the implementation: when caller passes no expiration,
            // AbsoluteExpirationRelativeToNow = 60 seconds, SlidingExpiration = null.
            var cache = new Mock<IDistributedCache>();
            DistributedCacheEntryOptions capturedOptions = null;
            cache.Setup(c => c.SetAsync(It.IsAny<string>(),
                                       It.IsAny<byte[]>(),
                                       It.IsAny<DistributedCacheEntryOptions>(),
                                       It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                    (_, __, options, ___) => capturedOptions = options)
                .Returns(Task.CompletedTask);

            await cache.Object.SetRecord("key", new Payload());

            capturedOptions.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromSeconds(60));
            capturedOptions.SlidingExpiration.Should().BeNull();
        }

        [Fact]
        public async Task SetRecord_WithExplicitAbsoluteExpiration_PassesItThrough()
        {
            var cache = new Mock<IDistributedCache>();
            DistributedCacheEntryOptions capturedOptions = null;
            cache.Setup(c => c.SetAsync(It.IsAny<string>(),
                                       It.IsAny<byte[]>(),
                                       It.IsAny<DistributedCacheEntryOptions>(),
                                       It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                    (_, __, options, ___) => capturedOptions = options)
                .Returns(Task.CompletedTask);

            await cache.Object.SetRecord("key", new Payload(), TimeSpan.FromMinutes(5));

            capturedOptions.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(5));
        }

        [Fact]
        public async Task SetRecord_WithExplicitSlidingExpiration_PassesItThrough()
        {
            var cache = new Mock<IDistributedCache>();
            DistributedCacheEntryOptions capturedOptions = null;
            cache.Setup(c => c.SetAsync(It.IsAny<string>(),
                                       It.IsAny<byte[]>(),
                                       It.IsAny<DistributedCacheEntryOptions>(),
                                       It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                    (_, __, options, ___) => capturedOptions = options)
                .Returns(Task.CompletedTask);

            await cache.Object.SetRecord("key", new Payload(),
                absoluteExpiredTime: TimeSpan.FromMinutes(10),
                unusedExpiredTime: TimeSpan.FromMinutes(2));

            capturedOptions.SlidingExpiration.Should().Be(TimeSpan.FromMinutes(2));
            capturedOptions.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(10));
        }

        // --- GetRecord -----------------------------------------------------------------------

        [Fact]
        public async Task GetRecord_WithCachedJson_DeserializesToTargetType()
        {
            var json = JsonSerializer.Serialize(new Payload { Name = "Riley", Score = 42 });
            var cache = new Mock<IDistributedCache>();
            cache.Setup(c => c.GetAsync("payload:1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(json));

            var result = await cache.Object.GetRecord<Payload>("payload:1");

            result.Should().NotBeNull();
            result.Name.Should().Be("Riley");
            result.Score.Should().Be(42);
        }

        [Fact]
        public async Task GetRecord_WhenCacheMisses_ReturnsDefaultValue()
        {
            // GetStringAsync returns null on cache miss; the extension forwards that as default(T).
            var cache = new Mock<IDistributedCache>();
            cache.Setup(c => c.GetAsync("missing", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            var refResult = await cache.Object.GetRecord<Payload>("missing");
            refResult.Should().BeNull();

            var valueResult = await cache.Object.GetRecord<int>("missing");
            valueResult.Should().Be(0); // default(int)
        }

        [Fact]
        public async Task GetRecord_WithCachedDictionary_DeserializesProperly()
        {
            var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
            var json = JsonSerializer.Serialize(dict);
            var cache = new Mock<IDistributedCache>();
            cache.Setup(c => c.GetAsync("dict", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(json));

            var result = await cache.Object.GetRecord<Dictionary<string, int>>("dict");

            result.Should().ContainKey("a").WhoseValue.Should().Be(1);
            result.Should().ContainKey("b").WhoseValue.Should().Be(2);
        }

        // --- Round-trip ----------------------------------------------------------------------

        [Fact]
        public async Task SetRecord_GetRecord_RoundTripsCustomType()
        {
            // Simulate a real cache by capturing what SetRecord stores and replaying it for GetRecord.
            // This proves the serialization format on the write path is compatible with the read path.
            var cache = new Mock<IDistributedCache>();
            byte[] storage = null;

            cache.Setup(c => c.SetAsync(It.IsAny<string>(),
                                       It.IsAny<byte[]>(),
                                       It.IsAny<DistributedCacheEntryOptions>(),
                                       It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                    (_, value, __, ___) => storage = value)
                .Returns(Task.CompletedTask);

            cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => storage);

            var input = new Payload { Name = "Riley", Score = 7 };
            await cache.Object.SetRecord("k", input);
            var output = await cache.Object.GetRecord<Payload>("k");

            output.Should().NotBeNull();
            output.Name.Should().Be("Riley");
            output.Score.Should().Be(7);
        }
    }
}
