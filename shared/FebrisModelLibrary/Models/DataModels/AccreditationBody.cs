// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class AccreditationBody:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        ///// <summary>
        ///// Generic Data
        ///// </summary>
        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        /// <summary>
        /// Link
        /// </summary>
        public AccreditationBodySettings AccreditationBodySettings { get; set; }
        public Guid AccreditationBodySettingsUUID { get; set; }

        /// <summary>
        /// Generic Data
        /// </summary>      
        [Display(Name = "This accreditation body is locked out")]
        public bool IsLockedOut { get; set; }


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
        /// This for test users
        /// </summary>
        [Display(Name = "Max Video Storage in Gigabytes")]
        public int MaxVideoStorage { get; set; }
        [Display(Name = "Max Test User Accounts (Typically 5)")]
        public int MaxTestUserAccounts { get; set; }

    }
}
