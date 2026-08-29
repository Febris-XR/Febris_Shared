// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// Scheduled follow-up against a Lead or Account -- "call John
    /// Tuesday at 3pm", "send proposal by Friday EOD". Replaces the
    /// previous reliance on <see cref="LeadRating"/>'s computed
    /// "ContactOverdue" bucket, which inferred neglect from absence of
    /// activity rather than capturing intent.
    /// <para>
    /// Exactly one of <see cref="LeadUUID"/> / <see cref="AccountUUID"/>
    /// is typically set, but neither is required at the schema level --
    /// some tasks (e.g., "review the pipeline this morning") aren't tied
    /// to a specific record. Both being null means a personal/admin task.
    /// </para>
    /// </summary>
    public class LeadTask : BaseModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        // ---- Ownership ----

        /// <summary>
        /// ApplicationUser.Id of the rep responsible for completing the
        /// task. The "Today / Overdue / Upcoming" admin view filters by
        /// this column for the signed-in user.
        /// </summary>
        [Required]
        [Display(Name = "Assigned to")]
        public Guid AssignedToUserId { get; set; }

        /// <summary>
        /// ApplicationUser.Id of whoever created the task. Useful when
        /// a manager assigns tasks to a rep -- distinguishes "I made
        /// this for myself" from "my boss asked me to do this".
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        // ---- Linkage ----

        /// <summary>
        /// Related Lead UUID, if the task concerns a specific lead.
        /// Mutually exclusive with <see cref="AccountUUID"/> in
        /// practice but the schema doesn't enforce it -- a task could
        /// reasonably reference both.
        /// </summary>
        public Guid? LeadUUID { get; set; }

        /// <summary>
        /// Related Account UUID, if the task is at the account level
        /// rather than tied to a specific contact.
        /// </summary>
        public Guid? AccountUUID { get; set; }

        /// <summary>
        /// CRM Phase 2 Tier 1 (2026-05-21): related Opportunity UUID,
        /// if the task is anchored to a specific deal ("send proposal to
        /// Acme by Friday"). Mutually compatible with
        /// <see cref="LeadUUID"/> / <see cref="AccountUUID"/> -- a task
        /// can be triple-linked when the rep wants it to appear on all
        /// three timelines.
        /// </summary>
        public Guid? OpportunityUUID { get; set; }

        // ---- Scheduling ----

        [Required]
        [Display(Name = "Due")]
        public DateTime DueAt { get; set; }

        /// <summary>Null while the task is open; set when the rep marks
        /// it done. Combined with <see cref="Status"/> so a task that
        /// was cancelled (Status=Cancelled) is distinguishable from one
        /// that was actually completed.</summary>
        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public LeadTaskPriority Priority { get; set; } = LeadTaskPriority.Medium;
        public LeadTaskStatus Status { get; set; } = LeadTaskStatus.Open;
    }
}
