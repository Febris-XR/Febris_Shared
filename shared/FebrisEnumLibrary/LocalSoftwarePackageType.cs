// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.EnumLibrary
{
    public enum LocalSoftwarePackageType
    {
        [Display(Name = "None")] None = 0,
        [Display(Name = "PC")] PC = 100,
        [Display(Name = "Mobile Server")] AndroidMobileServer = 200,
        [Display(Name = "Mobile Companion")] AndroidMobileCompanion = 300,
        [Display(Name = "C#")] CSharp = 400,
        [Display(Name = "C++")] CPP = 500
        
    }
}
