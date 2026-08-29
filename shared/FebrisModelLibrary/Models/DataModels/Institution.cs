// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class Institution : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; } // lets use this to link? otherwise it is not stated as needed

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        /// <summary>
        /// Link
        /// </summary>
        public InstitutionSettings InstitutionSettings { get; set; }
        public Guid? InstitutionSettingsUUID { get; set; }
        [Display(Name = "Institution Type")]
        public InstitutionType InstitutionType { get; set; }
        public Guid? InstitutionTypeUUID { get; set; }
        [Display(Name = "Deployment Type")]
        public DeploymentType DeploymentType { get; set; }
        public Guid? DeploymentTypeUUID { get; set; }

        /// <summary>
        /// Basic Data
        /// </summary>
        [Display(Name = "Institution Name")]
        public string Name { get; set; }
        [Display(Name = "Institution Address")]
        public string Address { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }        
        [Display(Name = "State")]
        public string State { get; set; }
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }
        [Display(Name = "Country")]
        public string Country { get; set; }


        /// <summary>
        /// Test User Setup
        /// </summary>
        [Display(Name = "Max Test User Accounts (Typically 5)")]
        public int MaxTestUserAccounts { get; set; } = 5;

        /// <summary>
        /// For map
        /// </summary>
        public double Longitude { get; set; }
        public double Latitude { get; set; }


        /// <summary>                
        /// Add License key here
        /// </summary>        
        //**May need to link this to the actual license************************************************
        //public Guid LicenseUUID { get; set; }
        //public License License { get; set; }

        public string Token { get; set; }

        //7 percent tax rate default
        public decimal TaxRate { get; set; } = 7;


        //Location lookup list
        //[JsonIgnore]
        //[IgnoreDataMember]
        //public List<InstitutionLinkedLocation> InstitutionLinkedLocationList { get; set; }

        ////Adding professionals to list of provider
        //public List<InstitutionLinkedProfessional> InstitutionLinkedProfessionalList { get; set; }

        ////sets up users with the ability to manage healthcare provider
        //public List<InstitutionLinkedUser> InstitutionLinkedUserList { get; set; }
    }
}
