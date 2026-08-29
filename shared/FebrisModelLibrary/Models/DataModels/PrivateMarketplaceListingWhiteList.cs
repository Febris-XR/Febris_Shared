// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class PrivateMarketplaceListingWhiteList : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //[Display(Name = "Creation date")]
        //public DateTime CreationTimeStamp { get; set; }

        //[Display(Name = "Last Modified")]
        //public DateTime UpdateTimeStamp { get; set; }


        public Institution Institution { get; set; }
        public Guid InstitutionUUID { get; set; }

        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }

        [Display(Name = "Approved By Institution")]
        public bool AcceptedByInstitution { get; set; }
        [Display(Name = "Approved By Developer")]
        public bool AcceptedByDeveloper { get; set; }
        [Display(Name = "Archived")]
        public bool Archived { get; set; }
        [Display(Name = "Active")]
        public bool Active { get; set; }

        //need to add more data here - could reference the last time the module/curriculum/listing is used
        public Guid RequestingUser { get; set; }
        public Guid ApprovingUser { get; set; }



    }    
}
