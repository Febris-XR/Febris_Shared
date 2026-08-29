// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.EnumLibrary
{
    public enum DisputeStatus
    {
        [Display(Name = "Not Set")] NotSet = 0,
        [Display(Name = "Recieved")] Recieved = 1001,
        //[Display(Name = "Unassigned")] Unassigned = 1001,
        [Display(Name = "Under Review")] UnderReview = 1987,
        [Display(Name = "In Progress")] InProgress = 4685,
        [Display(Name = "Resolved")] Resolved = 1685
    }

    public enum DisputeAction
    {
        [Display(Name = "Not Set")] NotSet = 0,
        [Display(Name = "None")] None = 4168,
        [Display(Name = "Issue found and corrected")] IssueFoundAndCorrected =1975,
        [Display(Name = "No refund required")] NoRefundRequired = 1741,
        [Display(Name = "Refunded")] Refunded = 1685
        
    }

    public enum IssueCategory
    {
        [Display(Name = "Not Set")] NotSet = 0,
        [Display(Name = "General")] General = 101,
        [Display(Name = "Cannot Claim Seats")] CannotClaimSeats = 201,

    }

    
}
