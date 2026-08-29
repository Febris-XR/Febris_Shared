// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// This can be found when the object is found 
    /// </summary>
    public class UsageDataByModule : BaseModel
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
        public Guid ModuleUUID { get; set; }
        public Module Module { get; set; }


        public int DailyUsageMeter { get; set; }
        public long TotalUsageMeter { get; set; }
    }
}
