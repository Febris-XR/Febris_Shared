// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class MarketplaceListingLinks
    {

    }    
    public class MarketplaceListingLinkedIndustry : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }
        public Industry Industry { get; set; }
        public Guid IndustryUUID { get; set; }
    }
    public class MarketplaceListingLinkedCategory : BaseModel
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }

        public DateTime CreationTimeStamp { get; set; }
        public DateTime UpdateTimeStamp { get; set; }

        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }
        public Category Category { get; set; }
        public Guid CategoryUUID { get; set; }
    }
    public class MarketplaceListingLinkedFocus : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }
        public Focus Focus { get; set; }
        public Guid FocusUUID { get; set; }
    }
    public class MarketplaceListingLinkedTag : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }
        public Tag Tag { get; set; }
        public Guid TagUUID { get; set; }
    }
    public class MarketplaceListingLinkedDiscount : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        /// <summary>
        /// not sure where to put this
        /// </summary>
        public bool IsActive { get; set; }

        public Guid MarketplaceListingUUID { get; set; }
        public MarketplaceListing MarketplaceListing { get; set; }

        public Guid DiscountUUID { get; set; }
        public Discount Discount { get; set; }
    }
    public class MarketplaceListingHardwareCompatibility : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public XRHardwareModel XRHardwareModel { get; set; }
        public Guid XRHardwareModelUUID { get; set; }        

        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }
    }

    public class MarketplaceListingFeedback:BaseModel
    {
        //user info
        public Guid UserId { get; set; }
        //other info
        public Institution Institution { get; set; }
        public Guid InstitutionUUID { get; set; }
        public AccreditationBody AccreditationBody { get; set; }
        public Guid AccreditationBodyUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
        //curriculum info
        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }
        //Feedback
        public int StarRating { get; set; }
        public string Title { get; set; }
        public string FeedbackComment { get; set; }
    }
}
