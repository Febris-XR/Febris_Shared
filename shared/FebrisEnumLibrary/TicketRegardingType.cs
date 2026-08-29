// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    //################################################################
    //################################################################
    public enum TicketRegardingType
    {
        [Display(Name = "General Question")] GeneralQuestion =0,
        [Display(Name = "Filter Request")] FilterRequest = 100,
        [Display(Name = "Feature Request")] FeatureRequest = 200,
        [Display(Name = "Billing")] Billing =300,
        [Display(Name = "Software Question")] SoftwareQuestion =400,
        [Display(Name = "Bug Report")] BugReport = 404,
    }
}
