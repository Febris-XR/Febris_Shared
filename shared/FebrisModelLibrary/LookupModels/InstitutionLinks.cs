// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class InstitutionLinks
    {
    }
    public class InstitutionSavedMarketplaceListing : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }
        public MarketplaceListing MarketplaceListing { get; set; }
        public Guid MarketplaceListingUUID { get; set; }
        public License License { get; set; }
        public Guid LicenseUUID { get; set; }
    }
    public class InstitutionLinkedUser : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Institution Institution { get; set; }
        public Guid InstitutionUUID { get; set; }
        public Guid UserId { get; set; }
        public AttachmentStatus AttachmentStatus { get; set; }
    }
    public class InstitutionLinkedProfessional : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        //public Professional Professional { get; set; }
        //public Guid ProfessionalUUID { get; set; }
        public Institution Institution { get; set; }
        public Guid InstitutionUUID { get; set; }
        public AttachmentStatus AttachmentStatus { get; set; }
    }
    public class InstitutionLinkedLocation : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Guid InstitutionUUID { get; set; }
        public Institution Institution { get; set; }
        public Guid LocationUUID { get; set; }
        public Location Location { get; set; }
    }
    public class InstitutionLinkedHardware : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Guid HardwareUUID { get; set; }
        public Hardware Hardware { get; set; }
        public Guid InstitutionUUID { get; set; }
        public Institution Institution { get; set; }
    }


}
