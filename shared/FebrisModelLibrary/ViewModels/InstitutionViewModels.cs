// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class InstitutionViewModels
    {
    }
    public class InstitutionCreationViewModel
    {
        public License License { get; set; }
        public Institution Institution { get; set; }
        public InstitutionSettings InstitutionSettings { get; set; }
        public bool RemoveLogo { get; set; }


        [Display(Name = "List of institution types")]
        public SelectList InstitutionTypeSelectList { get; set; }//may need to be select list
        public long? SelectedInstitutionTypeId { get; set; }

        [Display(Name = "List of deployment types")]
        public SelectList DeploymentTypeSelectList { get; set; }//may need to be select list
        public long? SelectedDeploymentTypeId { get; set; }

    }

}
