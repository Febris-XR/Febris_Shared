// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Audit T9: the device refresh token lived for EIGHT DAYS while its access token lived fifteen
    /// minutes, and rotation only happened inside the last 24 hours of that life. Every refresh
    /// before then handed back the SAME token, so a stolen one was usable for roughly a week and
    /// nothing noticed.
    ///
    /// <para>
    /// Owner ruling 2026-08-10: eight days is disproportionate for a short, internally-scoped
    /// process. The refresh token is now EIGHT HOURS, rotated on every refresh, and configurable.
    /// </para>
    ///
    /// <para>
    /// The defaults are the load-bearing part of this file. A node that configures nothing must get
    /// the SHORT lifetime, not the old one -- a fix that only applies when an operator opts in is
    /// not a fix.
    /// </para>
    /// </summary>
    public class JwtLifetimeSettingsTests
    {
        private static IConfiguration Config(Dictionary<string, string> values)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        private static IConfiguration Empty()
        {
            return Config(new Dictionary<string, string>());
        }

        [Fact]
        public void RefreshTokenDefaultsToEightHours_NotEightDays()
        {
            JwtLifetimeSettings.RefreshTokenLifetime(Empty())
                .Should().Be(TimeSpan.FromHours(8), "the ruling is 8 HOURS, and it must apply without any configuration");

            JwtLifetimeSettings.RefreshTokenLifetime(Empty())
                .Should().BeLessThan(TimeSpan.FromDays(1), "the whole point was to stop a stolen token being useful for a week");
        }

        [Fact]
        public void AccessTokenDefaultsToFifteenMinutes()
        {
            // Unchanged behaviour: this is the value the code hardcoded and the API's appsettings
            // already declared as 900 seconds.
            JwtLifetimeSettings.AccessTokenLifetime(Empty()).Should().Be(TimeSpan.FromMinutes(15));
        }

        [Fact]
        public void AccessTokenReadsTheKeyThatAlreadyExistedAndWasIgnored()
        {
            // JwtSettings:ExpiryTimeInSeconds was present in the API's appsettings at 900 and NOTHING
            // read it -- a knob wired to nothing. It is now the real source, so the existing config
            // surface starts working instead of being replaced by an invented key.
            JwtLifetimeSettings.AccessTokenLifetime(
                    Config(new Dictionary<string, string> { { "JwtSettings:ExpiryTimeInSeconds", "300" } }))
                .Should().Be(TimeSpan.FromMinutes(5));
        }

        [Fact]
        public void RefreshTokenIsConfigurable()
        {
            JwtLifetimeSettings.RefreshTokenLifetime(
                    Config(new Dictionary<string, string> { { "JwtSettings:RefreshTokenHours", "2" } }))
                .Should().Be(TimeSpan.FromHours(2));
        }

        [Fact]
        public void TheCacheRecordOutlivesTheTokenItself()
        {
            // Not cosmetic. Rotation writes the OLD token back marked Revoked, and that record is the
            // only evidence a presented token was already rotated out. If it evaporated the moment
            // the token expired, a replayed token would read as merely unknown rather than revoked.
            IConfiguration config = Empty();

            JwtLifetimeSettings.RefreshTokenCacheTtl(config)
                .Should().BeGreaterThan(JwtLifetimeSettings.RefreshTokenLifetime(config));
        }

        [Theory]
        [InlineData("banana")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("0")]
        [InlineData("-5")]
        public void GarbageOrNonPositiveValuesFallBackToTheDefault(string raw)
        {
            // A zero or negative would mint an already-expired token; unparseable input must not take
            // the host down or silently produce something absurd.
            JwtLifetimeSettings.RefreshTokenLifetime(
                    Config(new Dictionary<string, string> { { "JwtSettings:RefreshTokenHours", raw } }))
                .Should().Be(JwtLifetimeSettings.DefaultRefreshTokenLifetime);

            JwtLifetimeSettings.AccessTokenLifetime(
                    Config(new Dictionary<string, string> { { "JwtSettings:ExpiryTimeInSeconds", raw } }))
                .Should().Be(JwtLifetimeSettings.DefaultAccessTokenLifetime);
        }

        [Fact]
        public void ANullConfigurationDoesNotThrow()
        {
            JwtLifetimeSettings.RefreshTokenLifetime(null).Should().Be(JwtLifetimeSettings.DefaultRefreshTokenLifetime);
            JwtLifetimeSettings.AccessTokenLifetime(null).Should().Be(JwtLifetimeSettings.DefaultAccessTokenLifetime);
        }

        [Fact]
        public void FractionalValuesAreAccepted()
        {
            // "0.5" hours is a legitimate way to ask for thirty minutes.
            JwtLifetimeSettings.RefreshTokenLifetime(
                    Config(new Dictionary<string, string> { { "JwtSettings:RefreshTokenHours", "0.5" } }))
                .Should().Be(TimeSpan.FromMinutes(30));
        }
    }
}
