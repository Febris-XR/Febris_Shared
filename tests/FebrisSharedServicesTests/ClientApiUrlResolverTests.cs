// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;
using Febris.SharedServices.Launcher;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// ROADMAP 21 -- the PC background services could never reach the node.
    ///
    /// <para>
    /// <c>LocalHardwareStaticDetails.ApiUrl</c> was assigned only inside the PC Launcher and Mobile
    /// Server processes. It is a STATIC in the shared library and statics are per-process, so the
    /// statement uploader and module manager -- separate Topshelf service processes -- read a value
    /// nothing in their process had set. It stayed empty for their whole lifetime and every request
    /// resolved to a relative <c>"Token/..."</c>. No memory-mapped file carried the endpoint either.
    /// </para>
    ///
    /// <para>
    /// These pin both halves of the contract: a configured URL is applied, and an unconfigured or
    /// placeholder one fails CLOSED with a message an operator can act on. Failing closed is the
    /// severance rule -- a client that has not been told which node it belongs to must never guess,
    /// and must never fall back to a Febris host.
    /// </para>
    /// </summary>
    public class ClientApiUrlResolverTests
    {
        private static IConfiguration Config(params (string Key, string Value)[] pairs)
        {
            var dict = new Dictionary<string, string>();
            foreach ((string key, string value) in pairs)
            {
                dict[key] = value;
            }
            return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        }

        /// <summary>The target is a static, so each test starts from a known state.</summary>
        private static void Reset() => LocalHardwareStaticDetails.ApiUrl = string.Empty;

        [Fact]
        public void ConfiguredUrl_IsApplied()
        {
            Reset();
            ClientApiUrlResolver.TryApply(
                Config(("ApiUrlPath:DataApi", "https://node.example.org:5102/api/")), out string error)
                .Should().BeTrue();
            error.Should().BeNull();
            LocalHardwareStaticDetails.ApiUrl.Should().Be("https://node.example.org:5102/api/");
        }

        [Fact]
        public void MissingTrailingSeparator_IsAdded()
        {
            // The request builders concatenate directly ("Token/" + method), so without this a
            // configured "https://node/api" silently produces "https://node/apiToken/Refresh".
            Reset();
            ClientApiUrlResolver.TryApply(
                Config(("ApiUrlPath:DataApi", "https://node.example.org:5102/api")), out _).Should().BeTrue();
            LocalHardwareStaticDetails.ApiUrl.Should().Be("https://node.example.org:5102/api/");
        }

        [Fact]
        public void AuthenticationApi_IsUsedWhenDataApiIsAbsent()
        {
            Reset();
            ClientApiUrlResolver.TryApply(
                Config(("ApiUrlPath:AuthenticationApi", "https://node.example.org:5102/api/")), out _)
                .Should().BeTrue();
            LocalHardwareStaticDetails.ApiUrl.Should().Be("https://node.example.org:5102/api/");
        }

        // --- Fails closed: the severance rule ---

        [Fact]
        public void NoConfiguration_FailsClosed_AndLeavesApiUrlEmpty()
        {
            Reset();
            ClientApiUrlResolver.TryApply(null, out string error).Should().BeFalse();
            error.Should().Contain("ApiUrlPath");
            LocalHardwareStaticDetails.ApiUrl.Should().BeEmpty("a client that cannot be told its node must not guess one");
        }

        [Fact]
        public void EmptyConfiguration_FailsClosed_WithAnActionableMessage()
        {
            Reset();
            ClientApiUrlResolver.TryApply(Config(), out string error).Should().BeFalse();
            LocalHardwareStaticDetails.ApiUrl.Should().BeEmpty();
            error.Should().Contain("ApiUrlPath:DataApi", "the message must name the key an operator has to set");
            error.Should().Contain("ApiUrlPath__DataApi", "and the environment-variable form, which is how a service is configured");
        }

        [Theory]
        [InlineData("{NodeApiUrl}")]
        [InlineData("{ApiUrl}")]
        [InlineData("  {NodeApiUrl}  ")]
        public void UnsubstitutedPlaceholder_FailsClosed(string placeholder)
        {
            // The committed appsettings.json ships exactly this, deliberately. A deploy that forgets
            // to substitute it must not produce a client pointed at the literal string.
            Reset();
            ClientApiUrlResolver.TryApply(Config(("ApiUrlPath:DataApi", placeholder)), out _)
                .Should().BeFalse();
            LocalHardwareStaticDetails.ApiUrl.Should().BeEmpty();
        }

        [Fact]
        public void PlaceholderInPrimary_FallsThroughToAConfiguredFallback()
        {
            Reset();
            ClientApiUrlResolver.TryApply(
                Config(("ApiUrlPath:DataApi", "{NodeApiUrl}"),
                       ("ApiUrlPath:AuthenticationApi", "https://node.example.org:5102/api/")), out _)
                .Should().BeTrue();
            LocalHardwareStaticDetails.ApiUrl.Should().Be("https://node.example.org:5102/api/");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void BlankValue_FailsClosed(string blank)
        {
            Reset();
            ClientApiUrlResolver.TryApply(Config(("ApiUrlPath:DataApi", blank)), out _).Should().BeFalse();
            LocalHardwareStaticDetails.ApiUrl.Should().BeEmpty();
        }

        [Fact]
        public void ARealUrlContainingBraces_IsNotMistakenForAPlaceholder()
        {
            // The placeholder check is conservative on purpose: one outer brace pair, no nesting,
            // no whitespace. A URL is never that shape, but the guard should say so explicitly.
            Reset();
            ClientApiUrlResolver.TryApply(
                Config(("ApiUrlPath:DataApi", "https://node.example.org/{tenant}/api/")), out _)
                .Should().BeTrue();
            LocalHardwareStaticDetails.ApiUrl.Should().Be("https://node.example.org/{tenant}/api/");
        }
    }
}
