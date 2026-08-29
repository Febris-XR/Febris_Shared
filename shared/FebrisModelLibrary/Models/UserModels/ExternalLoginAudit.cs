// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.UserModels
{
    /// <summary>
    /// Enterprise Tier 1: one row per federated login attempt (SAML or
    /// OIDC), success or failure. Feeds two later surfaces:
    /// <list type="bullet">
    ///   <item><b>Operational debugging:</b> when an F500 IT team says
    ///   "our users can't log in," this is the log we read.</item>
    ///   <item><b>Audit log export:</b> F500 InfoSec reviews require
    ///   federated-login audit trails with >= 90 days of history.
    ///   Default retention is 365 days (config knob
    ///   <c>Authentication:FederatedAuditRetentionDays</c>); a
    ///   background sweep prunes older rows.</item>
    /// </list>
    /// <para>
    /// <b>Claims payload encryption:</b> <see cref="ClaimsPayloadEncrypted"/>
    /// stores the full received-claim dictionary as DataProtection-
    /// encrypted JSON. Admin troubleshooting can decrypt; raw DB
    /// exports (backups, replicas) cannot.
    /// </para>
    /// </summary>
    public class ExternalLoginAudit : BaseModel
    {
        [Required]
        [Display(Name = "Institution")]
        public Guid InstitutionUUID { get; set; }

        [Required]
        [Display(Name = "Protocol")]
        public Febris.EnumLibrary.IdentityProtocol Protocol { get; set; }

        [Display(Name = "External subject")]
        public string ExternalSubject { get; set; }

        [Display(Name = "Resolved user")]
        public Guid? UserId { get; set; }

        [Required]
        [Display(Name = "Success")]
        public bool Success { get; set; }

        [Display(Name = "Failure reason")]
        public string FailureReason { get; set; }

        [Display(Name = "Claims payload (encrypted)")]
        public string ClaimsPayloadEncrypted { get; set; }

        [Display(Name = "Remote IP")]
        public string RemoteIp { get; set; }

        [StringLength(500)]
        [Display(Name = "User agent")]
        public string UserAgent { get; set; }

        [Display(Name = "JIT-provisioned this attempt")]
        public bool JitProvisioned { get; set; }
    }
}
