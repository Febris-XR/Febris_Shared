// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins the CORS origin allow-list. The security-critical assertions are the rejections of
    /// look-alike ("evilexample.com") and suffix-injection ("example.com.evil.com") hosts, which
    /// a naive Contains/EndsWith-without-dot check would wrongly allow.
    /// <para>
    /// The allow-list is operator-supplied, so these tests configure it themselves rather than
    /// asserting against a domain baked into the product. The unconfigured case is covered
    /// separately and is the one that matters for a fresh self-hosted deployment.
    /// </para>
    /// </summary>
    public class CorsOriginPolicyTests
    {
        public CorsOriginPolicyTests()
        {
            // ".example.com" matches the bare domain and any subdomain of it.
            CorsOriginPolicy.AllowedHosts = new[] { ".example.com" };
        }

        [Theory]
        [InlineData("https://admin.example.com")]
        [InlineData("https://developer.example.com")]
        [InlineData("https://enduser.example.com")]
        [InlineData("https://marketing.example.com")]
        [InlineData("https://example.com")]
        [InlineData("https://EXAMPLE.COM")]
        [InlineData("https://Admin.Example.Com")]
        [InlineData("http://localhost")]
        [InlineData("http://localhost:3000")]
        [InlineData("http://127.0.0.1:5001")]
        public void IsFebrisOrigin_AllowsConfiguredSubdomainsAndLocalhost(string origin)
        {
            CorsOriginPolicy.IsFebrisOrigin(origin).Should().BeTrue();
        }

        [Theory]
        [InlineData("https://evil.com")]
        [InlineData("https://evilexample.com")]      // look-alike: no dot boundary before example.com
        [InlineData("https://example.com.evil.com")] // suffix injection: example.com is not the host suffix
        [InlineData("https://notexample.com")]
        [InlineData("https://sub.evil.com")]
        [InlineData("https://localhost.evil.com")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-valid-uri")]
        [InlineData(null)]
        public void IsFebrisOrigin_RejectsUnconfiguredAndMalformed(string origin)
        {
            CorsOriginPolicy.IsFebrisOrigin(origin).Should().BeFalse();
        }

        [Theory]
        [InlineData("https://admin.example.com")]
        [InlineData("https://example.com")]
        [InlineData("https://anything.at.all")]
        public void IsFebrisOrigin_WithNoConfiguredHosts_TrustsNoThirdPartyOrigin(string origin)
        {
            // The default for a freshly deployed node: nothing is configured, so no cross-origin
            // host is trusted. This is the assertion that would have failed against the old
            // hardcoded allow-list, which trusted one specific domain out of the box.
            CorsOriginPolicy.AllowedHosts = new string[0];

            CorsOriginPolicy.IsFebrisOrigin(origin).Should().BeFalse();
        }

        [Fact]
        public void IsFebrisOrigin_WithNoConfiguredHosts_StillAllowsLocalhost()
        {
            CorsOriginPolicy.AllowedHosts = new string[0];

            CorsOriginPolicy.IsFebrisOrigin("http://localhost:3000").Should().BeTrue();
        }
    }
}
