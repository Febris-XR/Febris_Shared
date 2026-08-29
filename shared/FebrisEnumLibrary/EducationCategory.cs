// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    public enum EducationCategory
    {
        [Display(Name = "VR Orientation")] Orientation,
        [Display(Name = "Onboard Education")] Onboarding,
        [Display(Name = "Palliative Education")] Palliative,
        [Display(Name = "Hospice Education")] Hospice,
        [Display(Name = "ICU Education")] ICUeducation,
        [Display(Name = "Pandemic")] Pandemic

    }
}
