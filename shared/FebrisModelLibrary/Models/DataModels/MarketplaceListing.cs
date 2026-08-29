// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// The marketplace listing model
    /// </summary>
    public class MarketplaceListing : BaseModel
    {        
        public string Name { get; set; }

        public Guid CurriculumUUID { get; set; }
        public Curriculum Curriculum { get; set; }

        [Display(Name = "Marketplace Listing Classification")]
        public MarketplaceListingClassification MarketplaceListingClassification { get; set; }
        public Guid MarketplaceListingClassificationUUID { get; set; }

        /// <summary>
        /// Curriculum Price
        /// </summary>
        [Display(Name = "Price in US Dollars")]
        public decimal Price { get; set; }

        /// <summary>
        /// if someone can buy the curriculum
        /// </summary>
        /// 
        [Display(Name = "Publish Listing (Public or Private)")]
        public bool Publish { get; set; }        
        public bool Obsolete { get; set; }
        [Display(Name = "Only for private use")]
        public bool Private { get; set; }
        
        /// <summary>
        /// Set a seat number limit. Best for demoing
        /// </summary>
        [Display(Name = "Set a seat number limit (Private listing only)")]
        public bool SetSeatNumberLimit { get; set; }
        [Display(Name = "Seat number limit")]
        public int? SeatNumberLimit { get; set; }

        [Display(Name = "Admin Lockout (only for use if copywrite infrenged or issues/concerns have arrisin)")]
        public bool AdminLockout { get; set; }
        /// <summary>
        /// apply the discount
        /// </summary>
        public bool ApplyDiscount { get; set; }
        
        ///Media
        public string VideoName { get; set; }
        public string ScreenShot1 { get; set; }
        public string ScreenShot2 { get; set; }
        public string ScreenShot3 { get; set; }
        public string ScreenShot4 { get; set; }
        public string ScreenShot5 { get; set; }

        public long? MarketplaceListingDiscountTiersId { get; set; }
        public Guid? MarketplaceListingDiscountTiersUUID { get; set; }
        public MarketplaceListingDiscountTiers MarketplaceListingDiscountTiers { get; set; }

        // Added by migration 20250819005016_InvoiceUpdates -- toggles the discount-tier pricing.
        public bool UseDiscountTiers { get; set; }
    }

    public class MarketplaceListingClassification:BaseModel
    {        

        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //[Display(Name = "Creation date")]
        //public DateTime CreationTimeStamp { get; set; }

        //[Display(Name = "Last Modified")]
        //public DateTime UpdateTimeStamp { get; set; }

        public bool Obsolete { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }


    public class MarketplaceListingDiscountTiers : BaseModel
    {
        // Tier-pack pricing: when a buyer's seat count >= TierNQty, seat price drops to TierNDiscountedPrice.
        // All nullable -- a listing may use 0..5 tiers depending on the publisher's pricing model.

        [Display(Name = "Tier 1 quantity")] public int? TierOneQty { get; set; }
        [Display(Name = "Tier 1 price")] public decimal? TierOneDiscountedPrice { get; set; }

        [Display(Name = "Tier 2 quantity")] public int? TierTwoQty { get; set; }
        [Display(Name = "Tier 2 price")] public decimal? TierTwoDiscountedPrice { get; set; }

        [Display(Name = "Tier 3 quantity")] public int? TierThreeQty { get; set; }
        [Display(Name = "Tier 3 price")] public decimal? TierThreeDiscountedPrice { get; set; }

        [Display(Name = "Tier 4 quantity")] public int? TierFourQty { get; set; }
        [Display(Name = "Tier 4 price")] public decimal? TierFourDiscountedPrice { get; set; }

        [Display(Name = "Tier 5 quantity")] public int? TierFiveQty { get; set; }
        [Display(Name = "Tier 5 price")] public decimal? TierFiveDiscountedPrice { get; set; }
    }


    //public class MarketplaceListingLinkedClassification
    //{
    //    public long Id { get; set; }
    //    public Guid UUID { get; set; }
    //    [Display(Name = "Creation date")]
    //    public DateTime CreationTimeStamp { get; set; }

    //    [Display(Name = "Last Modified")]
    //    public DateTime UpdateTimeStamp { get; set; }


    //    public MarketplaceListingClassification MarketplaceListingClassification { get; set; }
    //    public Guid MarketplaceListingClassificationUUID { get; set; }

    //    public MarketplaceListing MarketplaceListing { get; set; }
    //    public Guid MarketplaceListingUUID { get; set; }

    //}


}
