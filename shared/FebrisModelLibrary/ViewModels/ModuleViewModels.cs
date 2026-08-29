// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class ModuleViewModels
    {
    }
    public class ModuleCreationViewModel
    {
        //[Display(Name = "Pick the type of education being listed")]
        //public ListingType ListingType { get; set; }

        //[Display(Name = "Pick language this test is in (should be en-US)")]
        //public LanguageMapTypeEnum Language { get; set; }

        [Display(Name = "This is the IRI ID for this test object")]
        public Uri IRIId { get; set; }

        [Display(Name = "Alternate Test Name for xAPI definition")]
        public string AlternateName { get; set; }
        [Display(Name = "Alternate Test description for xAPI definition")]
        public string Alternatedescription { get; set; }

        [Display(Name = "Module File upload")]
        public string ModuleFile { get; set; }

        [Display(Name = "Select Module Industry")]
        public Guid? Industry { get; set; }
        public SelectList IndustryList { get; set; }

        [Display(Name = "Select Module Category")]
        public Guid? Category { get; set; }
        public SelectList CategoryList { get; set; }

        [Display(Name = "Select Module Focus")]
        public Guid? Focus { get; set; }
        public SelectList FocusList { get; set; }

        [Display(Name = "Select Relevant Tags")]
        public List<Guid?> SelectedTagList { get; set; }
        public SelectList TagList { get; set; }

        [Display(Name = "Select XR hardware types")]
        public List<Guid?> SelectedXRHardwareTypeList { get; set; }
        public SelectList XRHardwareTypeList { get; set; }


        [Display(Name = "Classification of this module")]
        public Guid? SelectedModuleClassification { get; set; }
        public SelectList ModuleClassificationList { get; set; }

        public Module Module { get; set; }

        public ContentDeveloper ContentDeveloper { get; set; }

        // File-upload integrity check (passed to FebrisSecurityMethods.CheckSumValidation).
        public Dictionary<string, string> Checksums { get; set; }
    }



}
