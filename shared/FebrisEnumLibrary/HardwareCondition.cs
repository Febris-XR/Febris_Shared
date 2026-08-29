// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    public enum HardwareCondition
    {
        [Display(Name = "Active")] Active,
        [Display(Name = "Not Active")] NotActive,        
    }
}
