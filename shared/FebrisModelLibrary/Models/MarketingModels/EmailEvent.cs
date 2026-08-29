// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// CRM Phase 1 (2026-05-20): email-engagement event recorded by the
    /// tracking pixel (Open) and link-redirect (Click) endpoints. One
    /// row per recipient-action; aggregations live in the reporting
    /// dashboard.
    /// <para>
    /// The pixel URL embeds <see cref="EmailCampaignMessageUUID"/> +
    /// <see cref="RecipientLeadUUID"/> so we can attribute the open to
    /// (campaign-message, recipient) without per-recipient pre-allocation.
    /// Click events also carry the <see cref="Url"/> the recipient followed.
    /// </para>
    /// <para>
    /// Privacy: <see cref="IpAddress"/> and <see cref="UserAgent"/> are
    /// captured for abuse detection + "preview-fetcher" filtering (some
    /// email clients fetch pixels server-side, inflating open rates).
    /// GDPR plumbing is Phase 1.5+ -- right now we store raw IP. A
    /// scheduled task can age the values out per retention policy when
    /// that lands.
    /// </para>
    /// </summary>
    public class EmailEvent : BaseModel
    {
        [Required]
        public Guid EmailCampaignMessageUUID { get; set; }

        /// <summary>UUID of the recipient Lead. Null for events that
        /// can't be attributed to a specific recipient (e.g., a generic
        /// transactional email opened via shared link).</summary>
        public Guid? RecipientLeadUUID { get; set; }

        [Required]
        public EmailEventType EventType { get; set; }

        /// <summary>For Click events, the target URL the recipient
        /// followed. Null for Open / Bounce / Unsubscribe.</summary>
        [StringLength(2048)]
        public string Url { get; set; }

        [StringLength(45)]
        public string IpAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }
    }
}
