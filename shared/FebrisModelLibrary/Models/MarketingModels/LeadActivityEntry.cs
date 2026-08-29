// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// CRM Phase 1 (2026-05-20): one row in the Lead-detail activity timeline.
    /// Unified Notes + Correspondence + Tasks feed; entries sort by Timestamp desc.
    /// Built by a helper on LeadController and rendered by Views/Lead/_ActivityTimeline.cshtml.
    /// </summary>
    public class LeadActivityEntry
    {
        public DateTime Timestamp { get; set; }
        public string Kind { get; set; }
        public string BadgeClass { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
