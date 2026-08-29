// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// This is more on correspondence with the lead and to see if they would like to be - this is more detailed than the notes area yet less free form
    /// 
    /// </summary>
    public class LeadCorrespondence:BaseModel
    {        
        public Guid MessagingUser { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }


        /// <summary>
        /// Previous contact information
        /// </summary>
        public DateTime? LastContactDate { get; set; }
        public ContactType LastContactType { get; set; }
        public bool RequestedFollowup { get; set; }

        /// <summary>
        /// Projected Contact 
        /// </summary>
        public bool NoFollowupNeeded { get; set; }
        public DateTime? FollowupDate { get; set; }
        public ContactType FollowupContactType { get; set; }



        ///add for one to many relationship
        //public long? LeadDetailsId { get; set; }
        public LeadDetails LeadDetails { get; set; }

        // CRM Tier 1 (task #87): optional link tying correspondence to a tracked Opportunity.
        public Guid? OpportunityUUID { get; set; }
    }
}
