// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Operator-declared reverse proxy trust.
    ///
    /// <para>
    /// This is not cosmetic. Four things read the client address, and one is a security control:
    /// analytics on every request, the two module-analytics readers, and the REFRESH TOKEN IP
    /// binding at <c>HardwareKeyAuthorization:133,:167</c>. Under the framework's loopback-only
    /// default with a proxy on any other address, all four see the PROXY's address, so the refresh
    /// binding becomes one constant shared by every device, which is no binding at all.
    /// </para>
    ///
    /// <para>
    /// The most important assertion in this file is the LAST one: absent configuration must behave
    /// exactly as before, so shipping this cannot silently re-trust a proxy for an operator who has
    /// not opted in.
    /// </para>
    /// </summary>
    public class ForwardedHeadersConfigurationTests
    {
        private static IConfiguration Config(Dictionary<string, string> values)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        [Fact]
        public void NoSection_KeepsTheFrameworkDefault_SoExistingDeploymentsAreUnaffected()
        {
            ForwardedHeadersOptions options = ForwardedHeadersConfiguration.Build(
                Config(new Dictionary<string, string>()));

            options.Should().NotBeNull();
            options.ForwardedHeaders.Should().Be(ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
            options.KnownNetworks.Should().NotBeEmpty("the loopback default must survive when nothing is configured");
        }

        [Fact]
        public void Disabled_ReturnsNull_SoTheCallerSkipsTheMiddlewareEntirely()
        {
            ForwardedHeadersOptions options = ForwardedHeadersConfiguration.Build(
                Config(new Dictionary<string, string> { { "ForwardedHeaders:Enabled", "false" } }));

            options.Should().BeNull();
        }

        [Fact]
        public void KnownNetworks_AcceptsACidrRange_ForAClusterPodNetwork()
        {
            // The Kubernetes answer: an ingress pod's address is assigned dynamically and cannot be
            // pinned, so the pod CIDR is what an operator can actually declare.
            ForwardedHeadersOptions options = ForwardedHeadersConfiguration.Build(
                Config(new Dictionary<string, string> { { "ForwardedHeaders:KnownNetworks:0", "10.42.0.0/16" } }));

            options.KnownNetworks.Should().ContainSingle();
            options.KnownNetworks.Single().Prefix.Should().Be(IPAddress.Parse("10.42.0.0"));
            options.KnownNetworks.Single().PrefixLength.Should().Be(16);
            options.KnownProxies.Should().BeEmpty("declaring a network replaces the loopback default rather than adding to it");
        }

        [Fact]
        public void KnownProxies_AcceptsLiteralAddresses()
        {
            ForwardedHeadersOptions options = ForwardedHeadersConfiguration.Build(
                Config(new Dictionary<string, string>
                {
                    { "ForwardedHeaders:KnownProxies:0", "192.168.1.10" },
                    { "ForwardedHeaders:KnownProxies:1", "192.168.1.11" }
                }));

            options.KnownProxies.Should().HaveCount(2);
            options.KnownProxies.Should().Contain(IPAddress.Parse("192.168.1.10"));
        }

        [Fact]
        public void ForwardLimit_DefaultsToOne_ButIsRaisableForALoadBalancerInFrontOfIngress()
        {
            ForwardedHeadersConfiguration.Build(Config(new Dictionary<string, string>
                { { "ForwardedHeaders:KnownProxies:0", "10.0.0.1" } }))
                .ForwardLimit.Should().Be(1, "one hop is the safe default");

            ForwardedHeadersConfiguration.Build(Config(new Dictionary<string, string>
                { { "ForwardedHeaders:ForwardLimit", "2" } }))
                .ForwardLimit.Should().Be(2, "LB -> ingress -> pod is TWO hops, and leaving it at 1 attributes traffic to the ingress");
        }

        [Fact]
        public void ForwardLimitZero_MeansUnlimited()
        {
            ForwardedHeadersConfiguration.Build(Config(new Dictionary<string, string>
                { { "ForwardedHeaders:ForwardLimit", "0" } }))
                .ForwardLimit.Should().BeNull();
        }

        [Fact]
        public void TrustAllProxies_ClearsBothLists()
        {
            // Safe only where the app is unreachable except through the ingress, which is true of
            // the bundled compose stack and of a typical cluster. NOT safe on a reachable host.
            ForwardedHeadersOptions options = ForwardedHeadersConfiguration.Build(
                Config(new Dictionary<string, string> { { "ForwardedHeaders:TrustAllProxies", "true" } }));

            options.KnownNetworks.Should().BeEmpty();
            options.KnownProxies.Should().BeEmpty();
        }

        [Fact]
        public void GarbageEntries_AreIgnored_AndDoNotTakeTheValidOnesWithThem()
        {
            // A typo in one CIDR must not throw at startup, and must not discard the entries that
            // parsed. Failing hard here would take the whole host down over a config typo.
            ForwardedHeadersOptions options = ForwardedHeadersConfiguration.Build(
                Config(new Dictionary<string, string>
                {
                    { "ForwardedHeaders:KnownNetworks:0", "10.42.0.0/16" },
                    { "ForwardedHeaders:KnownNetworks:1", "not-a-cidr" },
                    { "ForwardedHeaders:KnownNetworks:2", "10.43.0.0/999" },
                    { "ForwardedHeaders:KnownProxies:0", "192.168.1.10" },
                    { "ForwardedHeaders:KnownProxies:1", "banana" }
                }));

            options.KnownNetworks.Should().ContainSingle("only the valid CIDR survives");
            options.KnownProxies.Should().ContainSingle("only the valid address survives");
        }

        [Fact]
        public void AnEmptySection_DoesNotStripTheLoopbackDefault()
        {
            // The trap this guards: clearing the known-lists unconditionally would mean a present
            // but empty section silently turned into "trust everyone", which is the opposite of
            // what an operator writing an empty section intends.
            ForwardedHeadersOptions options = ForwardedHeadersConfiguration.Build(
                Config(new Dictionary<string, string> { { "ForwardedHeaders:ForwardLimit", "1" } }));

            options.KnownNetworks.Should().NotBeEmpty("an empty section must not be read as trust-everyone");
        }
    }
}
