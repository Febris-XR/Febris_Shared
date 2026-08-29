// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.UserModels
{
    /// <summary>
    /// SSO Tier 6: one row per identity event across the whole authentication surface -- local
    /// logins, MFA, password changes, role grants, IdP / SCIM administration, account lifecycle,
    /// and federated logins. This is the unified, queryable audit trail behind the AdminPortal
    /// "Identity audit log" and the SOC2 / ISO27001 evidence export. It complements
    /// <see cref="ExternalLoginAudit"/> (which stays federated-specific, with an encrypted claims
    /// payload); this table NEVER stores secrets -- no passwords, hashes, tokens, or raw claims.
    /// Append-only. See the SSO roadmap Tier 6.
    /// </summary>
    public class IdentityAuditEvent : BaseModel
    {
        [Required]
        [Display(Name = "Event type")]
        public IdentityAuditEventType EventType { get; set; }

        [Required]
        [Display(Name = "Success")]
        public bool Success { get; set; }

        // Guid.Empty is the sentinel for a Febris-global event (e.g. a SuperAdmin acting outside
        // any single institution). Otherwise the institution the event belongs to.
        [Display(Name = "Institution")]
        public Guid InstitutionUUID { get; set; }

        // Who initiated the event (the acting admin, or the user themselves). Null for anonymous
        // events (a failed login for an unknown email, a forgot-password request).
        [Display(Name = "Actor user")]
        public Guid? ActorUserId { get; set; }

        // Who/what the event is about. Null for non-user events (IdP / SCIM administration).
        [Display(Name = "Target user")]
        public Guid? TargetUserId { get; set; }

        // Email captured at event time, so the trail survives the user being renamed or deleted.
        [StringLength(256)]
        [Display(Name = "Target email")]
        public string TargetEmail { get; set; }

        // Small structured detail (short JSON or a phrase), e.g. {"role":"Admin"} or "authenticator".
        // MUST NOT contain secrets (passwords, hashes, tokens, raw claims).
        [Display(Name = "Details")]
        public string Details { get; set; }

        [Display(Name = "Failure reason")]
        public string FailureReason { get; set; }

        [Display(Name = "Remote IP")]
        public string RemoteIp { get; set; }

        [StringLength(500)]
        [Display(Name = "User agent")]
        public string UserAgent { get; set; }
    }
}
