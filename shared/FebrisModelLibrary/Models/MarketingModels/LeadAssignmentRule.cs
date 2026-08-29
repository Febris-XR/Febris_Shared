// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// CRM Phase 1 (2026-05-20): rep-assignment rule applied at intake
    /// time. When a new <see cref="LeadInbox"/> arrives and the
    /// downstream Lead has no AssignedRepresentative, the rules are
    /// evaluated in <see cref="Priority"/> order (lower number first)
    /// and the first match wins -- the Lead's
    /// <c>LeadDetails.AssignedRepresentative</c> is set to
    /// <see cref="AssignToUserId"/>.
    /// <para>
    /// Match semantics for Phase 1: <see cref="MatchLeadType"/> +
    /// <see cref="MatchInboxOption"/>. Both nullable -- null means
    /// "matches any value". A rule with both null is a catch-all
    /// (assign-to-self for the default rep). Future extensions can add
    /// UTM/country/free-webmail criteria without a schema change by
    /// hanging them off this same row.
    /// </para>
    /// <para>
    /// Rule editing is admin-only at the controller layer. Rules are
    /// soft-disabled via <see cref="IsActive"/> = false rather than
    /// deleted, so an inactive rule keeps its assignment history.
    /// </para>
    /// </summary>
    public class LeadAssignmentRule : BaseModel
    {
        /// <summary>Human-readable label for the admin UI (e.g.,
        /// "Healthcare leads -> Susan").</summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Evaluation order. Lower runs first. Ties broken by Id
        /// (insertion order). 0..1000 typical; gaps recommended to
        /// leave room for inserts (10, 20, 30 rather than 1, 2, 3).
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>Soft-disable flag. Inactive rules are skipped at
        /// evaluation time but kept for audit/history.</summary>
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // ---- Match criteria (all nullable = match-any) ----

        /// <summary>If set, rule only matches leads of this
        /// <see cref="LeadType"/>. Null = match any LeadType.</summary>
        [Display(Name = "Match Lead Type")]
        public LeadType? MatchLeadType { get; set; }

        /// <summary>If set, rule only matches when the LeadInbox row's
        /// <c>LeadInboxOptionList</c> contains this option. Null =
        /// match regardless of option.</summary>
        [Display(Name = "Match Inbox Option")]
        public LeadInboxOptions? MatchInboxOption { get; set; }

        // ---- CRM Phase 2 Tier 2.2 (2026-05-21): score-band match criteria ----

        /// <summary>
        /// Minimum lead score (0-100, from <c>LeadScoringLogic.ComputeScore</c>)
        /// to match. Null = no lower bound. Lets a "high-value" rule
        /// route hot leads to senior AEs while a "low-value" rule
        /// catches cold leads for BDR nurture.
        /// </summary>
        [Display(Name = "Min score to match")]
        [Range(0, 100)]
        public int? MinScoreToMatch { get; set; }

        /// <summary>
        /// Maximum lead score (0-100) to match. Null = no upper bound.
        /// Inclusive on both sides -- a rule with Min=40 Max=70 matches
        /// scores 40..70 inclusive.
        /// </summary>
        [Display(Name = "Max score to match")]
        [Range(0, 100)]
        public int? MaxScoreToMatch { get; set; }

        // ---- Action ----

        /// <summary>ApplicationUser.Id of the rep to assign matching
        /// leads to.</summary>
        [Required]
        [Display(Name = "Assign to user")]
        public Guid AssignToUserId { get; set; }
    }
}
