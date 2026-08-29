// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.EnumLibrary
{
    public enum TagType
    {
        [Display(Name = "Generic")] Generic,
        [Display(Name = "Industry")] Industry = 100,
        [Display(Name = "Category")] Category =200,
        [Display(Name = "Module")] Module =300,
        [Display(Name = "Curriculum")] Curriculum = 400,
        [Display(Name = "Education")] Education = 500,
        [Display(Name = "Other")] Other = 600,
    }
}
