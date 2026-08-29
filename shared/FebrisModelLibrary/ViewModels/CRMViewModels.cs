// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.MarketingModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class CRMViewModels
    {
    }

    public class BulkLeadCreationViewModel
    {        
        public SelectList LeadTagSelectList { get; set; }
        public List<Guid> SelectedTagList { get; set; }


        public SelectList LeadRatingSelectList { get; set; }
        public int SelectedLeadRating { get; set; }


        [Display(Name = "Lifecycle Stage")]
        public LifecycleStage LifecycleStage { get; set; }

        [Display(Name = "Lead Type")]
        public LeadType LeadType { get; set; }

    }
    public class BulkLeadCreationSubmitListViewModel
    {
        public List<Guid> TagList { get; set; }
        public LifecycleStage? LifecycleStage { get; set; }
        public int? LeadRating { get; set; }
        public LeadType? LeadType { get; set; }
        public List<BulkLeadCreationSubmitViewModel> SubmissionList { get; set; }
    }
    public class BulkLeadCreationSubmitViewModel
    {  
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobTitle { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Linkedin { get; set; }
        public string YouTubeUrl { get; set; }

        public string CompanyName { get; set; }
        public string CompanyWebsite { get; set; }        
        public string CompanyDescription { get; set; }
        //public string CompanyAddress { get; set; }
        //public string CompanyCity { get; set; }
        //public string CompanyState { get; set; }
        public string Industry { get; set; }
        public string CompanyValue { get; set; }
        public string EmployeeCount { get; set; }
        public string LeadSource { get; set; }
    }

    public class FAQViewModel
    {
        public List<FAQ> FAQList { get; set; }                       
        public FAQCategory FAQCategory { get; set; }
    }
    public class FAQViewModelList
    {
        public List<FAQViewModel> FAQViewModel { get; set; }
    }

    public class LeadCorrespondenceViewModel
    {
        public LeadCorrespondence LeadCorrespondence { get; set; }
        public Lead Lead { get; set; }
    }
}
