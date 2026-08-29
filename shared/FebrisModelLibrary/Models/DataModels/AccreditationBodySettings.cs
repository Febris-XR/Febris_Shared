// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class AccreditationBodySettings : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        
        ///// <summary>
        ///// Generic Data
        ///// </summary>        
        //public DateTime UpdateTimeStamp { get; set; }

        /// <summary>
        /// Toggles
        /// </summary>
        [Display(Name = "Allow Email Addresses Outside Of Domain")]
        public bool AllowEmailAddressesOutsideOfDomain { get; set; }        
        [Display(Name = "Force Multifactor Authentication")]
        public bool ForceMultifactorAuthentication { get; set; }

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
        /// Point of Contact
        /// </summary>
        [Display(Name = "Main Point of Contact First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Main Point of Contact Email")]
        public string POCEmail { get; set; }
        [Display(Name = "Main Point of Contact Phone number")]
        public string POCPhoneNumber { get; set; }
    }
}
