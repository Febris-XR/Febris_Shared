// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class UsageDataByCurriculum : BaseModel
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
        public Guid CurriculumUUID { get; set; }
        public Curriculum Curriculum { get; set; }


        public int DailyUsageMeter { get; set; }
        public long TotalUsageMeter { get; set; }

    }
}
