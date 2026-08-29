// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    public enum XRHardwareClass
    {
        [Display(Name = "Unknown")] Unknown = 0,
        [Display(Name = "Premium Headset")] Premium = 100,
        [Display(Name = "Stand-Alone Headset")] Standalone = 200,
        [Display(Name = "Phone Based")] PhoneBased = 300,
        [Display(Name = "Other")] Other = 400
    }
}
