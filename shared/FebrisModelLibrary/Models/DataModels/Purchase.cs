// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class PurchaseOrder : BaseModel
    {
        public bool Archived { get; set; }
        [Required]
        public Purchase Purchase { get; set; }
        public Guid PurchaseUUID { get; set; }
        public PurchaseDispute PurchaseDispute { get; set; }
        public Guid? PurchaseDisputeUUID { get; set; }
    }


    public class Purchase : BaseModel
    {

        //public bool Disputed { get; set; }
        //public bool Archive { get; set; }


        //by Instiution
        [Required]
        public License License { get; set; }
        [Required]
        public Guid LicenseUUID { get; set; }

        [Required]
        public Purchaser Purchaser { get; set; }

        [Required]
        public MarketplaceListing MarketplaceListing { get; set; }

        [Required]
        public Guid MarketplaceListingUUID { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Discount { get; set; }
        public bool HasBeenInvoiced { get; set; }
        public bool IsPrivateListing { get; set; }



        //Seat Allotment
        public bool Claimed { get; set; }
        public SeatAllotment SeatAllotment { get; set; }

    }

    public class PurchaseDispute : BaseModel
    {
        [Required]
        public License License { get; set; }
        [Required]
        public Guid LicenseUUID { get; set; }
        [Required]
        public Purchaser DisputingUser { get; set; }

        //General information
        public IssueCategory IssueCategory { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        //internal stuff        
        public DisputeStatus DisputeStatus { get; set; }
        public DisputeAction DisputeAction { get; set; }
        public EmployeeHandlingClaim EmployeeHandlingClaim { get; set; }
        //Employee notes
        public List<string> DisputeNoteList { get; set; }
               
    }
       
    [Owned]
    public class EmployeeHandlingClaim : BaseTimeStampModel
    {
        [Required]
        public Guid User { get; set; }
        [Required]
        public string EmailAddress { get; set; }
    }

    public class BaseTimeStampModel
    {
        [Display(Name = "Creation date")]
        public DateTime TimeStamp { get; set; }
        [Display(Name = "Last Modified")]
        public DateTime LastUpdateTimeStamp { get; set; }
    }

    [Owned]
    public class SeatAllotment : BaseTimeStampModel
    {
        public Guid ActorUUID { get; set; }
        public Guid UserUUID { get; set; }
    }

    [Owned]
    public class Purchaser //: BaseTimeStampModel
    {
        [Required]
        public Guid UserId { get; set; }
        //[Required]
        //public string EmailAddress { get; set; }
        [Required]
        public string Role { get; set; }
    }

        //public class Purchase : BaseModel
    //{
    //    //by Instiution
    //    public License License { get; set; }
    //    public Guid LicenseUUID { get; set; }

    //    //Purchasing User Info
    //    public Guid PurchasingUserId { get; set; }
    //    public string PurchasingUserEmail { get; set; }
    //    public string PurchasingUserRole { get; set; }

    //    //by purchase
    //    //public Guid LocalPurchaseUUID { get; set; } // can send the ones that do not exist back to query
    //    //public DateTime LocalPurchaseTimestamp { get; set; }
    //    //

    //    //Seat Allotment
    //    public Guid ActorUUID { get; set; }
    //    public Guid ProfessionalUUID { get; set; }


    //    //What is Purchased
    //    public MarketplaceListing MarketplaceListing { get; set; }
    //    public Guid MarketplaceListingUUID { get; set; }
    //    public decimal Price { get; set; }
    //    public int Discount { get; set; }
    //    public bool HasBeenInvoiced { get; set; }
    //}


}
