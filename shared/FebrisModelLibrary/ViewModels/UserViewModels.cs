// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.UserModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class UserViewModels
    {
    }

    #region user
    public class UserViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public Institution Institution { get; set; }
        public Location Location { get; set; }
        //public Professional Professional { get; set; }
        public ContentDeveloper ModuleDeveloper { get; set; }
        public string LocationName { get; set; }
        public string ModuleDeveloperName { get; set; }
        public bool IsLockedOut { get; set; }
        public long InstitutionId { get; set; }
        public string InstitutionName { get; set; }

        public string SearchString { get; set; }
        public string CurrentFilter { get; set; }
        
    }
   
    public class UserSettingsViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicturePath { get; set; }

        //public ApplicationUser ApplicationUser { get; set; }
        //public ApplicationRole ApplicationRole { get; set; }

        // File-upload integrity check (passed to FebrisSecurityMethods.CheckSumValidation).
        public Dictionary<string, string> Checksums { get; set; }
    }
    #endregion

    #region Febris
    public class FebrisUserCreation
    {
        public Guid Id { get; set; }
        //public string Name { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public FebrisUserType FebrisUserType { get; set; }

        //add a location? 
    }
    public class FebrisUserViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public bool IsLockedOut { get; set; }

    }
    #endregion

    #region Content Developer
    public class ContentDeveloperUserCreation
    {
        public Guid Id { get; set; }
        //public string Name { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        //public ContentDeveloperUserType ContentDeveloperUserType { get; set; }
        public UserAccountType UserAccountType { get; set; }

        public Guid ContentDeveloperId { get; set; }
    }
    public class ContentDeveloperUserCreationWithDropDowns
    {
        public ContentDeveloperUserCreation ContentDeveloperUserCreation { get; set; }
        [Display(Name = "List of Content Developers")]
        public SelectList ContentDeveloperSelectList { get; set; }
        public long? SelectedContentDeveloperId { get; set; }
    }
    public class ContentDeveloperUserViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public bool IsLockedOut { get; set; }
    }
    #endregion

    #region Accreditation Body
    public class AccreditationBodyUserViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public bool IsLockedOut { get; set; }
    }
    public class AccreditationBodyUserCreation
    {
        public Guid Id { get; set; }

        //public string Name { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        //public AccreditationBodyUserType AccreditationBodyUserType { get; set; }
        public UserAccountType UserAccountType { get; set; }

        public Guid AccreditationBodyId { get; set; }
    }
    public class AccreditationBodyUserCreationWithDropDowns
    {
        public AccreditationBodyUserCreation AccreditationBodyUserCreation { get; set; }

        //location dropdown
        [Display(Name = "List of Accreditation Bodies")]
        public SelectList AccreditationBodySelectList { get; set; }
        //[Required]
        public long? SelectedAccreditationBodyId { get; set; }

    }
    #endregion

    #region Local
    public class LocalUserCreation
    {
        public Guid Id { get; set; }        
        public Guid UserId { get; set; }
        public string IdentificationNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public InstitutionUserAccountType UserAccountType { get; set; }

        //add a location? 
    }
    public class LocalUserViewModel
    {
        public LocalApplicationUser ApplicationUser { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public bool IsLockedOut { get; set; }

    }
    //public class LocalUserViewModel
    //{
    //    public LocalApplicationUser ApplicationUser { get; set; }
    //    public Guid UserId { get; set; }
    //    public string Role { get; set; }
        
    //    public string LocationName { get; set; }
    //    public string ModuleDeveloperName { get; set; }
    //    public bool IsLockedOut { get; set; }
    //    public long InstitutionId { get; set; }
    //    public string InstitutionName { get; set; }

    //    public string SearchString { get; set; }
    //    public string CurrentFilter { get; set; }

    //}

    public class LocalUserSettingsViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdentificationNumber { get; set; }
        //public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicturePath { get; set; }
        public InstitutionUserAccountType UserAccountType { get; set; }

        //public ApplicationUser ApplicationUser { get; set; }
        //public ApplicationRole ApplicationRole { get; set; }       
    }
    #endregion


    public class BulkUserCreationViewModel
    {
        public SelectList CohortSelectList { get; set; }
        public List<Guid?> SelectedCohortList { get; set; }


        //public SelectList UserRoleSelectList { get; set; }
        //public int SelectedUserRole { get; set; }
        //[Display(Name = "Lifecycle Stage")]
        //public LifecycleStage LifecycleStage { get; set; }

        [Display(Name = "User Role")]
        public InstitutionUserAccountType? AccountType { get; set; }

    }
    public class BulkUserCreationSubmitListViewModel
    {
        public List<Guid?> SelectedCohortList { get; set; }
        public InstitutionUserAccountType AccountType { get; set; }
        //public int LeadRating { get; set; }
        //public LeadType LeadType { get; set; }
        public List<BulkUserCreationSubmitViewModel> SubmissionList { get; set; }
    }
    public class BulkUserCreationSubmitViewModel
    {
        public string IdentificationNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        
    }

    
}
