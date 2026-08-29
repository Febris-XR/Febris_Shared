// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class UsageDataByInstitution : BaseModel
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
        public Guid InstitutionUUID { get; set; }
        public Institution Institution { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }        

        public int DailyModuleUsageMeter { get; set; }
        public long TotalModuleUsageMeter { get; set; }

        public int DailyCurriculumUsageMeter { get; set; }
        public long TotalCurriculumUsageMeter { get; set; }
    }    
}
