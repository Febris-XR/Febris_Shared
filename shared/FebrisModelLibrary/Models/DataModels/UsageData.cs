// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// Record data for billing and breakdown
    /// </summary>
    public class UsageData : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        ///// <summary>
        ///// Dates
        ///// </summary>
        //public DateTime TimeStamp { get; set; }


        /// <summary>
        /// Pertaining to
        /// </summary>
        //public AccountType AccountType { get; set; }
        public Guid? ContentDeveloperUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid? InstitutionUUID { get; set; }
        public Institution Institution { get; set; }
        public Guid? AccreditationBodyUUID { get; set; }
        public AccreditationBody AccreditationBody { get; set; }



        /// <summary>
        /// Sales
        /// </summary>
        public int UnitCount { get; set; }
        public decimal MoneyEarned { get; set; }


        /// <summary>
        /// maybe break it down by customer and content used
        /// </summary>

        public License License { get; set; }
        public Guid LicenseUUID { get; set; }


    }
}
