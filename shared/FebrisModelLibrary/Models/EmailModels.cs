// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.Models.EmailModels
{
    // Email transport shapes. Relocated from Febris.SharedServices (EmailService.cs)
    // per the "models + view models live in FebrisModelLibrary" rule (R1).
    public class TestEmailViewModel
    {
        public EmailModel EmailModel { get; set; }
        public HostEmailProperties HostEmailProperties { get; set; }
        public EmailType? EmailType { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
    public class HostEmailProperties
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string Sender { get; set; }
        public string Password { get; set; }
        public string SenderName { get; set; }

        /// <summary>
        /// Whether to perform an online certificate-revocation check on the SMTP server's
        /// certificate. Defaults to FALSE, which is the behaviour that actually shipped and worked
        /// for years.
        ///
        /// <para>
        /// WHY THIS EXISTS. MailKit 2.10.1 defaulted <c>CheckCertificateRevocation</c> to false.
        /// The NET8 package bump to MailKit 4.16.0 flipped that default to TRUE, and nothing in
        /// this codebase set it either way, so the behaviour changed silently with no code edit.
        /// When the OCSP or CRL responder cannot be reached, MailKit then REJECTS an otherwise
        /// valid certificate and every send fails with "the remote certificate was rejected", while
        /// openssl on the same machine validates that same certificate cleanly. The observed
        /// failure names the cause outright: "the revocation function was unable to check
        /// revocation because the revocation server was offline".
        /// </para>
        ///
        /// <para>
        /// It also explains why this looked intermittent rather than broken: when the responder does
        /// answer, the send succeeds, so it works on most attempts and fails on some.
        /// </para>
        ///
        /// <para>
        /// Defaulting to false matters most for the self-hosted node, the deployment least likely
        /// to have outbound OCSP available. Behind a restrictive firewall, an intercepting proxy, or
        /// air-gapped, an unreachable responder is the NORMAL case, so leaving this on would mean
        /// password reset and user invites never work at all rather than failing occasionally. The
        /// certificate chain and host name are still fully validated either way. An operator with
        /// reliable OCSP egress who wants revocation enforced sets
        /// <c>EmailSender:CheckCertificateRevocation</c> to true.
        /// </para>
        /// </summary>
        public bool CheckCertificateRevocation { get; set; }
    }
    public class EmailModel
    {
        public string RecipientName { get; set; }
        public string RecipientEmailAddress { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string SpecialHyperlink { get; set; }
        public Guid RecipientUUID { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
}
