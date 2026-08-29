// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;
using Febris.SharedServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins the SMTP transport settings, and specifically the one that broke silently.
    ///
    /// <para>
    /// THE REGRESSION THIS EXISTS FOR. Mail worked for years on MailKit 2.10.1, which defaulted
    /// <c>CheckCertificateRevocation</c> to false. The NET8 package bump to MailKit 4.16.0 flipped
    /// that default to true. No code changed, no config changed, and nothing in this codebase set
    /// the property either way, so the behaviour changed silently with the package version. When the
    /// OCSP or CRL responder is unreachable MailKit then rejects an otherwise valid certificate and
    /// every send fails with "the remote certificate was rejected", while openssl on the same
    /// machine validates the same certificate cleanly. The failure names the cause outright: "the
    /// revocation function was unable to check revocation because the revocation server was
    /// offline".
    /// </para>
    ///
    /// <para>
    /// It presented as intermittent rather than broken, because a reachable responder lets the send
    /// through. That is why it survived "just about every test".
    /// </para>
    ///
    /// <para>
    /// These assertions are about the BINDING, not about reaching a mail server. They run offline
    /// and open no socket.
    /// </para>
    /// </summary>
    public class EmailTransportSettingsTests
    {
        private static IConfiguration Config(Dictionary<string, string> values)
        {
            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        [Fact]
        public void RevocationChecking_DefaultsToOff_WhenTheKeyIsAbsent()
        {
            // The whole point. An operator upgrading, or a fresh self-hosted node whose config
            // predates the key, must get the behaviour that worked rather than the MailKit 4
            // default that does not.
            EmailService service = new EmailService(Config(new Dictionary<string, string>
            {
                { "EmailSender:Host", "smtp.example.com" },
                { "EmailSender:Port", "587" },
                { "EmailSender:Sender", "node@example.com" },
                { "EmailSender:Password", "unused" },
                { "EmailSender:SenderName", "Febris" }
            }));

            service.HostEmailProperties.CheckCertificateRevocation.Should().BeFalse(
                "MailKit 4 defaults this ON and rejects valid certificates whenever the revocation "
                + "responder is unreachable, which is the normal case for a firewalled or air-gapped node");
        }

        [Fact]
        public void RevocationChecking_CanBeTurnedOn_ByAnOperatorWithReliableOcspEgress()
        {
            EmailService service = new EmailService(Config(new Dictionary<string, string>
            {
                { "EmailSender:Host", "smtp.example.com" },
                { "EmailSender:Port", "587" },
                { "EmailSender:CheckCertificateRevocation", "true" }
            }));

            service.HostEmailProperties.CheckCertificateRevocation.Should().BeTrue();
        }

        [Fact]
        public void TheRestOfTheTransportStillBinds()
        {
            // Guards against the new key being spliced in at the cost of an existing one.
            EmailService service = new EmailService(Config(new Dictionary<string, string>
            {
                { "EmailSender:Host", "smtp.example.com" },
                { "EmailSender:Port", "2525" },
                { "EmailSender:EnableSSL", "true" },
                { "EmailSender:Sender", "node@example.com" },
                { "EmailSender:Password", "secret" },
                { "EmailSender:SenderName", "Node" }
            }));

            service.HostEmailProperties.Host.Should().Be("smtp.example.com");
            service.HostEmailProperties.Port.Should().Be(2525);
            service.HostEmailProperties.EnableSSL.Should().BeTrue();
            service.HostEmailProperties.Sender.Should().Be("node@example.com");
            service.HostEmailProperties.SenderName.Should().Be("Node");
        }
    }
}
