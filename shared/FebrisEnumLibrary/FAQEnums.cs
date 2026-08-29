// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.EnumLibrary
{
    class FAQEnums
    {
    }
    public enum FAQCategory
    {
        [Display(Name = "General Questions")] General,
        [Display(Name = "Educators")] Educators = 100,
        [Display(Name = "Healthcare")] Healthcare = 200,
        [Display(Name = "Developers")] Developers = 300,
        [Display(Name = "IT")] IT = 400,
        [Display(Name = "Our Technology")] Technology = 500,
        [Display(Name = "Content Development")] ContentDevelopment = 600,
        [Display(Name = "Marketplace")] Marketplace = 700,
    }
}
