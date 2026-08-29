// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    public class Lead : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //[Display(Name = "Lead Creation Date")]
        //public DateTime TimeStamp { get; set; }
        //[Display(Name = "Last Update")]
        //public DateTime UpdateTimeStamp { get; set; }

        public LeadDetails LeadDetails { get; set; }
        public Guid LeadDetailsUUID { get; set; }



        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        //contact info

        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
               

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Lead Source")]
        public string LeadSource { get; set; }       //change to enum?

        // CRM Phase 1 (task #22): optional link to the parent Account.
        public Guid? AccountUUID { get; set; }

        // CRM Lead-uniqueness Tier A (task #92): canonical email used for
        // duplicate detection (trim + lower + plus-strip + Gmail dot-fold).
        public string NormalizedEmail { get; set; }

        // CRM Phase 1 (task #24): marketing-attribution fields captured from
        // the public marketing site at intake. UTM parameters mirror the
        // standard Google Analytics UTM schema; Referrer is the HTTP
        // Referer header; Source is the parsed lead-source enum.
        public string Referrer { get; set; }
        public Febris.EnumLibrary.LeadSources? Source { get; set; }
        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public string UtmContent { get; set; }
        public string UtmTerm { get; set; }
    }
}
