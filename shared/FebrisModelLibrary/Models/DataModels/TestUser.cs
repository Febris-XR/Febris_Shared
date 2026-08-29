// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class TestUser : BaseModel
    {
        /// <summary>
        /// Link
        /// </summary>
        /// 
        public string UserName { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Identification Number (use a recognizable number this is unique otherwise a randomly generated number will be assigned)")]
        public string IdentificationNumber { get; set; }
        [Display(Name = "Your Headshot")]
        public string PhotoOfProfessional { get; set; }
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public Guid ActorId { get; set; }

        public string EmailAddress { get; set; }
    }
}
