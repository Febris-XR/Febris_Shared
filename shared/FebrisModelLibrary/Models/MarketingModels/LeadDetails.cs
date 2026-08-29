// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    
    public class LeadDetails : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }        
        public LeadType LeadType { get; set; }
        public bool Archive { get; set; }
        public bool Unsubscribed { get; set; }


        //Attaching to existing organization
        public Guid LinkGuid { get; set; }
        public long LinkId { get; set; }
        public Guid? License { get; set; }

        //rateing
        public LeadRating LeadRating { get; set; }
        public Guid LeadRatingUUID { get; set; }


        //sales data
        [Display(Name = "Assigned Representative")]
        public Guid AssignedRepresentative { get; set; }
        [Display(Name = "Lifecycle Stage")]
        public LifecycleStage LifecycleStage { get; set; }        
        [Display(Name = "Is Best Point Of Contact")]
        public bool IsBestPointOfContact { get; set; }


        //company data
        [Display(Name = "Company Value in dollars")]
        public decimal CompanyValue { get; set; }
        [Display(Name = "Number of Employees")]
        public int NumberOfEmployees { get; set; }
        public string Industry { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }


        //contact information
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }


        [Display(Name = "Office number")]
        public string OfficePhoneNumber { get; set; }
        [Display(Name = "Cell number")]
        public string CellPhoneNumber { get; set; }



        //social media accounts
        public string Facebook { get; set; }
        public string YouTube { get; set; }
        public string Instagram { get; set; }
        public string TikTok { get; set; }
        public string Twitter { get; set; }
        public string LinkedIn { get; set; }
        public string Reddit { get; set; }

        //Other operaitons
        public List<LeadCorrespondence> LeadCorrespondenceList { get; set; }
    }

    
}
