// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class ContentDeveloperLinks
    {
    }

    public class ContentDeveloperLinkedUser : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Guid UserId { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
    }
    public class ContentDeveloperLinkedModule : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Module Module { get; set; }
        public Guid ModuleUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
    }
    //public class ContentDeveloperLinkedHardware : BaseModel
    //{
    //    //public long Id { get; set; }
    //    //public Guid UUID { get; set; }

    //    //public DateTime CreationTimeStamp { get; set; }
    //    //public DateTime UpdateTimeStamp { get; set; }

    //    public Guid HardwareUUID { get; set; }
    //    public Hardware Hardware { get; set; }
    //    public Guid ContentDeveloperUUID { get; set; }
    //    public ContentDeveloper ContentDeveloper { get; set; }
    //}
    public class ContentDeveloperLinkedDiscount : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Guid ContentDeveloperUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid DiscountUUID { get; set; }
        public Discount Discount { get; set; }
    }
    public class ContentDeveloperLinkedCurriculum : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Curriculum Curriculum { get; set; }
        public Guid CurriculumUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
    }
    //public class ContentDeveloperLinkedAccreditationBody
    //{
    //    public long Id { get; set; }
    //    public Guid UUID { get; set; }

    //    public DateTime CreationTimeStamp { get; set; }
    //    public DateTime UpdateTimeStamp { get; set; }

    //    public AccreditationBody AccreditationBody { get; set; }
    //    public Guid AccreditationBodyUUID { get; set; }
    //    public ContentDeveloper ContentDeveloper { get; set; }
    //    public Guid ContentDeveloperUUID { get; set; }
    //}
    public class ContentDeveloperLinkedMarketplaceListing : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
    }
}
