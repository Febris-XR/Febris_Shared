// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.UserModels
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        //not sure if this is needed.
        public ApplicationUser() : base() { }

        //public bool HasProfilePicture { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicturePath { get; set; }
        public Guid? ContentDeveloper { get; set; }
        public Guid? AccreditationBody { get; set; }
        public Guid? Institution { get; set; }                        
        public Guid? LiabilityWaiver { get; set; }
        public Guid? ServiceAgreement { get; set; }

        public Guid? EULA { get; set; }
        //public Guid Professional { get; set; }        
        //public Guid Actor { get; set; }
    }
    public class LocalApplicationUser : IdentityUser<Guid>
    {
        //not sure if this is needed.
        public LocalApplicationUser() : base() { }

        //public bool HasProfilePicture { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicturePath { get; set; }
        public Guid? Institution { get; set; }
        public Guid? LiabilityWaiver { get; set; }
        public Guid? ServiceAgreement { get; set; }
        public Guid? EULA { get; set; }
        public Guid? Actor { get; set; }
        public string IdentificationNumber { get; set; }

        /// <summary>
        /// Soft-delete flag (AccountLifecycle.SoftDeleteOnly). A soft-deleted account is RETAINED (for xAPI
        /// history / FERPA) but locked out (cannot sign in); its email/username stays reserved -- the row is
        /// still visible to UserManager.FindByEmail so re-registration is cleanly rejected -- until an
        /// operator purges it (AccountLifecycle.PurgeAfterDays).
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>UTC time the account was soft-deleted; null while active. Drives PurgeAfterDays.</summary>
        public DateTimeOffset? DeletedUtc { get; set; }
    }
}
