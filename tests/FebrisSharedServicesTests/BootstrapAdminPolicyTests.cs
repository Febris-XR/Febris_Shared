// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins BootstrapAdminPolicy.ResolveEmail -- the first-boot admin fail-closed rule (audit B-03):
    /// a configured email always wins, otherwise dev/staging fall back to a local default and
    /// production resolves to null so the seed never invents a production admin.
    /// </summary>
    public class BootstrapAdminPolicyTests
    {
        [Theory]
        [InlineData("ops@febr.is")]
        [InlineData("  ops@febr.is  ")]
        public void ConfiguredEmail_Wins_Trimmed_RegardlessOfEnvironment(string configured)
        {
            BootstrapAdminPolicy.ResolveEmail(configured, devOrStaging: false, devFallback: "admin@febr.is")
                .Should().Be("ops@febr.is");
            BootstrapAdminPolicy.ResolveEmail(configured, devOrStaging: true, devFallback: "admin@febr.is")
                .Should().Be("ops@febr.is");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Production_FailsClosed_WhenEmailUnset(string configured)
        {
            BootstrapAdminPolicy.ResolveEmail(configured, devOrStaging: false, devFallback: "admin@febr.is")
                .Should().BeNull("production must never invent a bootstrap admin");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void DevOrStaging_FallsBackToDefault_WhenEmailUnset(string configured)
        {
            BootstrapAdminPolicy.ResolveEmail(configured, devOrStaging: true, devFallback: "admin@febr.is")
                .Should().Be("admin@febr.is");
        }
    }
}
