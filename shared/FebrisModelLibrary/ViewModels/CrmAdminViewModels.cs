// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.MarketingModels;
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels
{
    // AdminPortal CRM view models. Relocated from the AdminPortal controllers
    // (AccountController / TaskController / OpportunityController) per the
    // "models + view models live in FebrisModelLibrary" rule (R1).
    // Only MarketingModels is imported so `Account` resolves to the CRM
    // Account (not the xAPI Models.XApiModels.Account).

    /// <summary>
    /// Account row with its associated Leads + open Tasks.
    /// </summary>
    public class AccountDetailsViewModel
    {
        public Account Account { get; set; }
        public List<Lead> AssociatedLeads { get; set; }
        public List<LeadTask> OpenTasks { get; set; }
    }

    /// <summary>
    /// View model for the tab-style Task Index. Purely a controller-to-view transport.
    /// </summary>
    public class TaskIndexViewModel
    {
        public List<LeadTask> Today { get; set; }
        public List<LeadTask> Overdue { get; set; }
        public List<LeadTask> Upcoming { get; set; }
    }

    /// <summary>
    /// CRM Phase 2 Tier 1 (2026-05-21): viewmodel for the Opportunity
    /// detail page -- bundles the entity + its line items + assembled
    /// activity timeline so the view can render everything without a
    /// second controller round-trip.
    /// </summary>
    public class OpportunityDetailsViewModel
    {
        public Opportunity Opportunity { get; set; }
        public List<OpportunityLineItem> LineItems { get; set; }

        /// <summary>
        /// Unified activity entries (notes + correspondence + tasks)
        /// filtered to this Opportunity. Sorted Timestamp DESC by the
        /// controller before reaching the view.
        /// </summary>
        public List<OpportunityActivityEntry> Activity { get; set; } = new List<OpportunityActivityEntry>();
    }

    /// <summary>
    /// CRM Phase 2 Tier 1 (2026-05-21): one row in the Opportunity-detail
    /// activity timeline. Mirrors the Lead-side <c>LeadActivityEntry</c>
    /// but uses the Wave-2 .fb-badge CSS classes since the Opportunity
    /// detail view is built on the new design tokens.
    /// </summary>
    public class OpportunityActivityEntry
    {
        public DateTime Timestamp { get; set; }
        public string Kind { get; set; }
        public string BadgeClass { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
