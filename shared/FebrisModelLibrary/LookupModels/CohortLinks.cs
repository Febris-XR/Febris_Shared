// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    //class CohortLinks
    //{
    //}

    //public class CohortLinkedProfessional
    //{
    //    public long Id { get; set; }
    //    public Guid UUID { get; set; }

    //    public DateTime CreationTimeStamp { get; set; }
    //    public DateTime UpdateTimeStamp { get; set; }

    //    public Cohort Cohort { get; set; }
    //    public Guid CohortUUID { get; set; }

    //    public Professional Professional { get; set; }
    //    public Guid ProfessionalUUID { get; set; }
    //}


    public class CohortLinkedLocation:BaseModel
    {
        public Cohort Cohort { get; set; }
        public Guid CohortUUID { get; set; }


        public Location Location { get; set; }
        public Guid LocationUUID { get; set; }

    }

    public class CohortLinkedCurriculum:BaseModel
    {
        public Cohort Cohort { get; set; }
        public Guid CohortUUID { get; set; }
        public Curriculum Curriculum { get; set; }
        public Guid CurriculumUUID { get; set; }
    }




}
