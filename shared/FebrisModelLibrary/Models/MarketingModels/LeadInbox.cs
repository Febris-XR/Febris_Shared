// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    public class LeadInbox : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; } // lets use this to link? otherwise it is not stated as needed
        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        //just a message       
        public string Subject { get; set; }
        public string MessageBody { get; set; }

        //internal use
        public bool Important { get; set; }
        public bool Read { get; set; }

        //user selection
        public bool Demo { get; set; }
        //public bool ContentDevelopment { get; set; }
        //public bool CompanyRegistration { get; set; }
        //public bool SomethingElse { get; set; }


        //public bool JoinMailingList { get; set; }

        public List<LeadInboxOptions> LeadInboxOptionList { get; set; }

        public TicketStatusType TicketStatusType { get; set; }

        public Lead Lead { get; set; }

        // CRM Lead-uniqueness Tier A (task #92): when an intake matches an
        // existing Lead by NormalizedEmail, this points at the cluster owner
        // so the admin merge-duplicates UI can surface it.
        public Guid? PossibleDuplicateOfLeadUUID { get; set; }

        // CRM Phase 1 (task #24): UTM intake captured from the inbound
        // HTTP referer + query string by ContactUsPartial.cshtml hidden
        // inputs; carried through to the persisted Lead.
        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public string UtmTerm { get; set; }
        public string UtmContent { get; set; }

        // Raw HTTP Referer header at intake (alongside the UTM params).
        public string Referrer { get; set; }
    }

    public class LeadInboxViewModel
    {
        [Display(Name = "Connect")]
        public bool Connect { get; set; }
        [Display(Name = "Ask A Question")]
        public bool Question { get; set; }
        [Display(Name = "Periodic Updates")]
        public bool Updates { get; set; }
        [Display(Name = "Pricing Information")]
        public bool Pricing { get; set; }
        [Display(Name = "Get A Demo")]
        public bool Demo { get; set; }
        [Display(Name = "Get Febris For Your Company")]
        public bool CompanyRegistration { get; set; }
        [Display(Name = "Develop Our Curriculum")]
        public bool CustomContentDevelopment { get; set; }
        [Display(Name = "Content Developer Application")]
        public bool ContentDeveloperApplication { get; set; }
        [Display(Name = "Accreditation Body Application")]
        public bool AccreditationBodyApplication { get; set; }


        public LeadInbox LeadInbox { get; set; }

        // S-09 hardening (task #31): reCAPTCHA v3 token from the marketing
        // intake form; verified server-side by IRecaptchaVerifier before
        // the LeadInbox row is persisted.
        public string RecaptchaToken { get; set; }
    }


    public class ContactUsViewModel
    {
        public LeadInboxOptions LeadInboxOptions { get; set; }
    }
}
