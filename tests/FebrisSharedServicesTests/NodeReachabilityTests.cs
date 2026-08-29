// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Net;
using System.Net.Sockets;
using Febris.SharedServices.Launcher;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// The PC background services gated their work on an ICMP ping to a hardcoded public address,
    /// <c>8.8.8.8</c> in both services and <c>google.com</c> in the copies they were cloned from.
    ///
    /// <para>
    /// The work behind that gate is downloading modules from the node and uploading statements and
    /// video to it, so the only useful question is whether the NODE answers. Pinging a public
    /// resolver answered a different question and got this one wrong three ways: an air-gapped
    /// deployment has a reachable node and no route to Google, so the gate stayed shut forever and
    /// the service silently did nothing. A network blocking outbound ICMP failed identically while
    /// every HTTP call would have worked. And a reachable resolver with a dead node opened the gate
    /// on evidence about somebody else's server.
    /// </para>
    ///
    /// <para>
    /// These pin the half that can be asserted deterministically. <c>TryGetEndpoint</c> is pure, so
    /// it carries most of the coverage, and the socket probe is exercised against a listener this
    /// test owns rather than against anything on the network.
    /// </para>
    /// </summary>
    public class NodeReachabilityTests
    {
        [Theory]
        [InlineData("https://node.example.org:5102/api/", "node.example.org", 5102)]
        [InlineData("http://node.example.org:8080/api/", "node.example.org", 8080)]
        [InlineData("https://node.example.org/api/", "node.example.org", 443)]
        [InlineData("http://node.example.org/api/", "node.example.org", 80)]
        [InlineData("https://10.0.0.7:5102/api/", "10.0.0.7", 5102)]
        [InlineData("http://localhost:5000/", "localhost", 5000)]
        [InlineData("  https://node.example.org:5102/api/  ", "node.example.org", 5102)]
        [InlineData("https://node.example.org:5102", "node.example.org", 5102)]
        public void TryGetEndpoint_AbsoluteHttpUrl_YieldsHostAndPort(string url, string expectedHost, int expectedPort)
        {
            NodeReachability.TryGetEndpoint(url, out string host, out int port).Should().BeTrue();
            host.Should().Be(expectedHost);
            port.Should().Be(expectedPort, "a URL without an explicit port still has a scheme default");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryGetEndpoint_NoValue_IsRefused(string url)
        {
            NodeReachability.TryGetEndpoint(url, out string host, out int port).Should().BeFalse();
            host.Should().BeNull();
            port.Should().Be(0);
        }

        [Theory]
        [InlineData("Token/Refresh")]
        [InlineData("/api/")]
        [InlineData("api/Token/")]
        public void TryGetEndpoint_RelativeValue_IsRefused(string url)
        {
            // This is the exact shape an unconfigured service used to produce, so it is rejected
            // rather than coerced into something that looks probeable.
            NodeReachability.TryGetEndpoint(url, out _, out _).Should().BeFalse();
        }

        [Theory]
        [InlineData("ftp://node.example.org/")]
        [InlineData("file:///C:/node/")]
        [InlineData("{ApiUrl}")]
        [InlineData("not a url at all")]
        public void TryGetEndpoint_NonHttpValue_IsRefused(string url)
        {
            NodeReachability.TryGetEndpoint(url, out _, out _).Should().BeFalse();
        }

        [Fact]
        public void IsNodeReachable_Unconfigured_FailsClosed()
        {
            // The severance rule. No configured node means nothing to probe, and the answer is
            // "do not attempt node work", never "assume it is fine".
            NodeReachability.IsNodeReachable(null).Should().BeFalse();
            NodeReachability.IsNodeReachable("").Should().BeFalse();
            NodeReachability.IsNodeReachable("Token/Refresh").Should().BeFalse();
        }

        [Fact]
        public void IsNodeReachable_SomethingListening_IsTrue()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                NodeReachability.IsNodeReachable($"http://127.0.0.1:{port}/api/", 2000)
                    .Should().BeTrue("the probe opens a socket to the configured host and port");
            }
            finally
            {
                listener.Stop();
            }
        }

        [Fact]
        public void IsNodeReachable_NothingListening_IsFalse()
        {
            // Claim a port, then release it, so we hold a number nothing is bound to. Loopback
            // refuses immediately rather than waiting out the timeout.
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            NodeReachability.IsNodeReachable($"http://127.0.0.1:{port}/api/", 2000).Should().BeFalse();
        }

        [Fact]
        public void IsNodeReachable_NonPositiveTimeout_FallsBackToTheDefault()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                NodeReachability.IsNodeReachable($"http://127.0.0.1:{port}/api/", 0).Should().BeTrue();
                NodeReachability.IsNodeReachable($"http://127.0.0.1:{port}/api/", -5).Should().BeTrue();
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}
