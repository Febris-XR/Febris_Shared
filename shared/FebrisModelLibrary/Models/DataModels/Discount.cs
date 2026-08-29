// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// can't tell if this is really needed. I may be able to get rid of it and just put it directly in the marketplace listing
/// </summary>
namespace Febris.ModelLibrary.Models.DataModels
{
    public class Discount : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }


        public int PercentDiscount { get; set; }
        public decimal CapitalDiscount { get; set; }
        public int MaxNumberOfDiscounts { get; set; }
    }
}
