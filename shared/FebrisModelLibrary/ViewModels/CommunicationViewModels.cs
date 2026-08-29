// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Interfaces.ViewModelInterfaces;
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.MarketingModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class CommunicationViewModels
    {
    }

    public class JwtSettings : IJwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }
        public double ExpiryTimeInSeconds { get; set; }
    }

    public class MessageBoardViewModel
    {
        public string SearchString { get; set; }
        public string CurrentFilter { get; set; }
        public string StatusMessage { get; set; }

        public MessageBoard MessageBoard { get; set; }
        public List<MessageBoard> MessageBoardList { get; set; }
        public Institution Institution { get; set; }
        public ContentDeveloper ModuleDeveloper { get; set; }

        public string InstitutionName { get; set; }
        public string LocationName { get; set; }
        public long? InstitutionId { get; set; }
        public long? LocationId { get; set; }


        //location dropdown
        [Display(Name = "List of Locations")]
        public SelectList LocationList { get; set; }
        //[Required]
        public long? SelectedLocationId { get; set; }

        [Display(Name = "List of Institutions")]
        public SelectList InstitutionList { get; set; }
        //[Required]
        public long? SelectedInstitutionId { get; set; }

        [Display(Name = "List of Module Developers")]
        public SelectList ModuleDeveloperList { get; set; }
        //[Required]
        public long? SelectedModuleDeveloperId { get; set; }
    }
       
}
