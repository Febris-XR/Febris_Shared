// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// CRM Phase 2 Tier 1 (2026-05-21): the deal entity. An Opportunity
    /// represents a tracked sales pursuit -- dollar value, expected
    /// close date, stage progression, win/loss outcome.
    /// <para>
    /// Phase 1 left a structural gap: the <see cref="LifecycleStage"/>
    /// enum named <c>Opportunity</c> and <c>Customer</c> as stages, but
    /// nothing backed them. This entity closes that gap.
    /// </para>
    /// <para>
    /// Linkage:
    /// <list type="bullet">
    ///   <item><see cref="AccountUUID"/> -- required; the parent company.</item>
    ///   <item><see cref="PrimaryContactLeadUUID"/> -- nullable; the
    ///   buyer-side champion at the Account. Set automatically on
    ///   Lead-&gt;Opportunity conversion.</item>
    ///   <item><see cref="OwnerUserId"/> -- required; the AE
    ///   responsible. Drives auth via
    ///   <c>OwnershipChecks.EnsureOwnerOrAdmin</c>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <see cref="Amount"/> is the sum of <see cref="OpportunityLineItem"/>
    /// totals; <c>OpportunityLogic.RecomputeAmount</c> updates it after
    /// every line-item create/update/delete.
    /// </para>
    /// </summary>
    public class Opportunity : BaseModel
    {
        // ---- Identity ----

        /// <summary>
        /// Short display name shown in lists (e.g. "Acme Q3 expansion").
        /// Required.
        /// </summary>
        [Required]
        [StringLength(200)]
        [Display(Name = "Opportunity name")]
        public string Name { get; set; }

        // ---- Linkage ----

        /// <summary>Parent <see cref="Account"/>. Required.</summary>
        [Required]
        [Display(Name = "Account")]
        public Guid AccountUUID { get; set; }

        /// <summary>
        /// Buyer-side champion (<see cref="Lead"/>) at the parent Account.
        /// Nullable -- multi-stakeholder deals may not have a single
        /// designated champion; conversion from a single Lead sets this.
        /// </summary>
        [Display(Name = "Primary contact")]
        public Guid? PrimaryContactLeadUUID { get; set; }

        /// <summary>
        /// ApplicationUser.Id of the rep responsible for the deal.
        /// Drives the per-rep pipeline view in Tier 2 reports.
        /// </summary>
        [Required]
        [Display(Name = "Owner")]
        public Guid OwnerUserId { get; set; }

        // ---- Stage + probability ----

        /// <summary>Current stage. See <see cref="DealStage"/>.</summary>
        [Required]
        [Display(Name = "Stage")]
        public DealStage DealStage { get; set; } = DealStage.Prospecting;

        /// <summary>
        /// Win probability 0-100. Defaulted from <see cref="DealStage"/>
        /// on stage transitions but stays per-deal overridable (enterprise
        /// deals at the same stage can carry genuinely different
        /// confidence). Used by Tier 2 weighted-forecast reports.
        /// </summary>
        [Range(0, 100)]
        [Display(Name = "Stage probability (%)")]
        public int StageProbability { get; set; } = 10;

        // ---- Money + dates ----

        /// <summary>
        /// Total deal value (currency = <see cref="Currency"/>).
        /// Computed: sum of <see cref="OpportunityLineItem.LineTotal"/>
        /// across this Opportunity's line items.
        /// </summary>
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// ISO 4217 currency code (USD, EUR, GBP, ...). Default USD.
        /// Forward-defensive -- avoids a future breaking migration if
        /// multi-currency becomes a requirement.
        /// </summary>
        [StringLength(3)]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// When the rep expects to close. Drives Tier 2's forecast
        /// month-by-month report.
        /// </summary>
        [Required]
        [Display(Name = "Expected close date")]
        public DateTime ExpectedCloseDate { get; set; }

        /// <summary>
        /// When the deal actually closed. Set by
        /// <c>OpportunityLogic.MarkWon</c> / <c>MarkLost</c>. Null while
        /// the deal is still open.
        /// </summary>
        [Display(Name = "Actual close date")]
        public DateTime? ActualCloseDate { get; set; }

        // ---- Outcome ----

        /// <summary>
        /// Why the deal was lost. Only meaningful when
        /// <see cref="DealStage"/> = <see cref="DealStage.ClosedLost"/>.
        /// Tier 2 "Loss-reason breakdown" report pivots on this.
        /// </summary>
        [Display(Name = "Loss reason")]
        public OpportunityLossReason? LossReason { get; set; }

        // ---- Working notes ----

        /// <summary>
        /// Short free-text next action ("call CFO next Tuesday").
        /// Shown prominently in pipeline kanban cards so the rep can
        /// scan their pipeline without opening each deal.
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Next step")]
        public string NextStep { get; set; }

        // ---- Soft delete ----

        /// <summary>
        /// Soft-archive flag (instead of hard delete). Pipeline history
        /// has analytical value even for abandoned deals; the Index view
        /// hides Archived deals by default but they're still queryable.
        /// </summary>
        public bool Archived { get; set; }

        // ---- EF navigation properties (not persisted on this row) ----

        /// <summary>
        /// Line items belonging to this opportunity. EF-Include populated;
        /// not always loaded.
        /// </summary>
        public List<OpportunityLineItem> LineItems { get; set; }
    }
}
