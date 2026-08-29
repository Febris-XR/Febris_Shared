// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    public class LeadNote : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }
        public bool Archive { get; set; }

        public Guid NoteWriterId { get; set; }

        public Lead Lead { get; set; }
        public Guid LeadUUID { get; set; }

        public ContactType ContactType { get; set; }
        //public LeadContact LeadContact { get; set; }
        //public Guid LeadContactUUID { get; set; }

        public LeadMessage LeadMessage { get; set; }
        public Guid LeadMessageUUID { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }

        // CRM Tier 1 (task #87): optional link tying a note to a tracked Opportunity.
        public Guid? OpportunityUUID { get; set; }
    }
}
