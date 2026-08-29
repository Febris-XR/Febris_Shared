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
    class CurriculumViewModels
    {
    }

    public class CurriculumCreationViewModel
    {
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

        [Display(Name = "Select Included Modules Tags")]
        public List<Guid?> SelectedModuleList { get; set; }
        public SelectList ModuleList { get; set; }

        [Display(Name = "Classification of this Curriculum")]
        public Guid? SelectedCurriculumClassification { get; set; }
        public SelectList CurriculumClassificationList { get; set; }


        public Curriculum Curriculum { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }

    }

}
