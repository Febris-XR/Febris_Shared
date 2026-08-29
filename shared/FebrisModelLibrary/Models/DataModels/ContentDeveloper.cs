// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Interfaces.DataModelInterfaces;
using Febris.ModelLibrary.LookupModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class ContentDeveloper : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; } // lets use this to link? otherwise it is not stated as needed

        ///// <summary>
        ///// Generic Data
        ///// </summary>
        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        /// <summary>
        /// Link
        /// </summary>
        public ContentDeveloperSettings ContentDeveloperSettings { get; set; }
        public Guid? ContentDeveloperSettingsUUID { get; set; }
        public ContentDeveloperType ContentDeveloperType { get; set; }

        /// <summary>
        /// Basic Data
        /// </summary>
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }
        [Display(Name = "State")]
        public string State { get; set; }
        [Display(Name = "Country")]
        public string Country { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        
        /// <summary>
        /// For connection to lms if it exists
        /// </summary>
        [Display(Name = "Connection Token")]
        public string ConnectionToken { get; set; }
        [Display(Name = "This content developer is locked out")]
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// This for test users
        /// </summary>
        [Display(Name = "Max Video Storage in Gigabytes")]
        public int MaxVideoStorage { get; set; }
        [Display(Name = "Max Test User Accounts (Typically 5)")]
        public int MaxTestUserAccounts { get; set; }


        /// <summary>
        /// payment info
        /// </summary>
        public bool PaymentInfoVerified { get; set; }

        public decimal ServiceChargeRate { get; set; } = 30;

        // Added by migration 20250819005016_InvoiceUpdates -- per-publisher subscription rate.
        public decimal SubscriptionRate { get; set; }

        // SSO self-signup flow (task #10 migration 20260520172937_AddContentDeveloperPendingSelfSignUp):
        // true while a content-developer org is awaiting Febris admin approval after
        // /Identity/Account/Register; flipped to false (and IsLockedOut to false)
        // by IContentDeveloperLogic.ApprovePendingSelfSignUpAsync from the
        // AdminPortal Moderation > Pending developers queue.
        public bool PendingSelfSignUp { get; set; }

        //lookup list
        //[JsonIgnore]
        //[IgnoreDataMember]
        //public List<ContentDeveloperLinkedModule> ModuleDeveloperLinkedModuleList { get; set; }

        //public List<ContentDeveloperUser> ModuleDeveloperUserList { get; set; }
    }
}
