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
    class ContentDeveloperViewModels
    {
    }

    public class ContentDeveloperCreationViewModel
    {
        
        public ContentDeveloper ContentDeveloper { get; set; }
        public ContentDeveloperSettings ContentDeveloperSettings { get; set; }

        public bool RemoveLogo { get; set; }

        [Display(Name = "List of content developer types")]
        public SelectList ContentDeveloperTypeSelectList { get; set; }//may need to be select list
        public long? SelectedContentDeveloperTypeId { get; set; }
    }

    public class ContentDeveloperSelfEditViewModel
    {
        public ContentDeveloper ContentDeveloper { get; set; }
        public bool RemoveLogo { get; set; }
        // File-upload integrity check (passed to FebrisSecurityMethods.CheckSumValidation).
        public Dictionary<string, string> Checksums { get; set; }
    }






}
