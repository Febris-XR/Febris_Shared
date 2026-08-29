// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins ReturnUrlPolicy.IsAllowedPortalOrigin -- the open-redirect guard (audit B-01) that decides
    /// whether an absolute post-login returnUrl points at an allowed Febris portal origin. The page
    /// still handles local URLs via IUrlHelper.IsLocalUrl; this covers the absolute-origin allowlist,
    /// including the host-suffix-spoof, scheme, and port edge cases an attacker would try.
    /// </summary>
    public class ReturnUrlPolicyTests
    {
        private static readonly string[] Portals =
        {
            "https://admin.example.com",
            "https://developer.example.com"
        };

        [Theory]
        [InlineData("https://admin.example.com")]
        [InlineData("https://admin.example.com/")]
        [InlineData("https://admin.example.com/dashboard?tab=1")]
        [InlineData("https://developer.example.com/modules")]
        [InlineData("https://ADMIN.example.com/x")]
        public void Allows_AbsoluteUrls_OnAConfiguredPortalOrigin(string returnUrl)
        {
            ReturnUrlPolicy.IsAllowedPortalOrigin(returnUrl, Portals)
                .Should().BeTrue($"{returnUrl} is on a configured portal origin");
        }

        [Theory]
        [InlineData("https://evil.example")]
        [InlineData("https://evil.example/admin.example.com")]
        [InlineData("https://admin.example.com.evil.com/x")] // host-suffix spoof
        [InlineData("http://admin.example.com/x")]           // scheme mismatch
        [InlineData("https://admin.example.com:8443/x")]     // port mismatch
        [InlineData("//evil.example")]                            // protocol-relative, not absolute
        [InlineData("/Manage")]                                   // relative/local -- not this method's job
        [InlineData("not a url")]
        [InlineData("")]
        [InlineData(null)]
        public void Rejects_NonPortalOrAttackerControlledTargets(string returnUrl)
        {
            ReturnUrlPolicy.IsAllowedPortalOrigin(returnUrl, Portals)
                .Should().BeFalse($"{returnUrl} is not an allowed portal origin");
        }

        [Fact]
        public void Rejects_WhenAllowlistEmptyOrNull()
        {
            ReturnUrlPolicy.IsAllowedPortalOrigin("https://admin.example.com", new string[0]).Should().BeFalse();
            ReturnUrlPolicy.IsAllowedPortalOrigin("https://admin.example.com", null).Should().BeFalse();
        }
    }
}
