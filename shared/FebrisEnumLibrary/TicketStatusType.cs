// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    public enum TicketStatusType
    {
        [Display(Name = "Not Opened")] NotYetOpened ,
        [Display(Name = "Checked Out")] CheckedOut = 100,
        [Display(Name = "Responded")] Responded = 200,
        [Display(Name = "Unresolved")] Unresolved = 300,
        [Display(Name = "Resolved")] Resolved = 400,
        [Display(Name = "Closed Unresolved")] ClosedUnresolved =500, 
    }
}
