// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// This needs to be reworked
    /// </summary>
    public class License : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        ///// <summary>
        ///// Guid to connecting to any of the account types
        ///// </summary>
        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Guid LicenseKey { get; set; }
        public AccountType AccountType { get; set; }
        public bool AccountLocked { get; set; }

        public Institution Institution { get; set; }
        public Guid InstitutionUUID { get; set; }
    }
}
