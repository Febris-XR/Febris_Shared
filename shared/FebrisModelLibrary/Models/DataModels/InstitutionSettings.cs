// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class InstitutionSettings : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime UpdateTimeStamp { get; set; }

        /// <summary>
        /// Link
        /// </summary>
        //public Institution Institution { get; set; }
        //public Guid InstitutionUUID { get; set; }

        /// <summary>
        /// Toggles
        /// </summary>
        [Display(Name = "Auto Download Library Across Hardware")]
        public bool AutoDownloadLibraryAcrossHardware { get; set; }
        [Display(Name = "Auto Connect All Library Curriculum")]
        public bool AutoConnectAllLibraryCurriculum { get; set; }
        [Display(Name = "Is State Tax Exempt")]
        public bool IsStateTaxExempt { get; set; }
        [Display(Name = "Is Federally Tax Exempt")]
        public bool IsFederallyTaxExempt { get; set; }
        [Display(Name = "Is Non-Profit")]
        public bool IsNonProfit { get; set; }
        [Display(Name = "Allow Migrations")]
        public bool AllowMigrations { get; set; }
        [Display(Name = "Allow Email Addresses Outside Of Domain")]
        public bool AllowEmailAddressesOutsideOfDomain { get; set; }   
        [Display(Name = "Force Multifactor Authentication")]
        public bool ForceMultifactorAuthentication { get; set; }
        [Display(Name = "Allow Private Marketplace")]
        public bool AllowPrivateDeployments { get; set; }


        /// <summary>
        /// password validity
        /// </summary>
        [Display(Name = "Force Password Validity Timespan")]
        public bool ForcePasswordValidityTimespan { get; set; }
        [Display(Name = "Password Validity In Months")]
        public int PasswordValidityInMonths { get; set; }


        /// <summary>
        /// Compliance
        /// </summary>
        [Display(Name = "Needs To Be FERPA compliant (Th will run more slowly)")]
        public bool FERPACompliance { get; set; }
        

        /// <summary>
        /// inputs
        /// </summary>
        [Display(Name = "Email Domain")]
        public string EmailDomain { get; set; }
        [Display(Name = "Logo")]
        public string Logo { get; set; }
        [Display(Name = "Website")]
        public string Website { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// Video Storage
        /// </summary>
        [Display(Name = "Opt in for video storage")]
        public bool VideoStorageOption { get; set; }
        [Display(Name = "Use Video Strorage Time Span")]
        public bool UseVideoStrorageTimeSpan { get; set; }
        [Display(Name = "Length, in Months, of Video Storage")]
        public int VideoStorageTimeSpan { get; set; }
        [Display(Name = "Use Max Video Strorage")]
        public bool UseMaxVideoStorage { get; set; }
        [Display(Name = "Max Video Storage in Gigabytes")]
        public int MaxVideoStorage { get; set; }

        /// <summary>
        /// Point of Contact
        /// </summary>
        [Display(Name = "Main Point of Contact First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Main Point of Contact Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Main Point of Contact Email")]
        public string POCEmail { get; set; }
        [Display(Name = "Main Point of Contact Phone number")]
        public string POCPhoneNumber { get; set; }


    }
}
