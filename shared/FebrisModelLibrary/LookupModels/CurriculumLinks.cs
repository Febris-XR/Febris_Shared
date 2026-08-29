// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class CurriculumLinks
    {
    }
    public class CurriculumLinkedIndustry : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Curriculum Curriculum { get; set; }
        public Guid CurriculumUUID { get; set; }
        public Industry Industry { get; set; }
        public Guid IndustryUUID { get; set; }
    }
    public class CurriculumLinkedCategory : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Curriculum Curriculum { get; set; }
        public Guid CurriculumUUID { get; set; }
        public Category Category { get; set; }
        public Guid CategoryUUID { get; set; }
    }
    public class CurriculumLinkedFocus : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Curriculum Curriculum { get; set; }
        public Guid CurriculumUUID { get; set; }
        public Focus Focus { get; set; }
        public Guid FocusUUID { get; set; }
    }
    public class CurriculumLinkedTag : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Curriculum Curriculum { get; set; }
        public Guid CurriculumUUID { get; set; }
        public Tag Tag { get; set; }
        public Guid TagUUID { get; set; }
    }
    public class CurriculumHardwareCompatibility : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public XRHardwareModel XRHardwareModel { get; set; }
        public Guid XRHardwareModelUUID { get; set; }        
        public Curriculum Curriculum { get; set; }
        public Guid CurriculumUUID { get; set; }
    }
    public class CurriculumFeedback : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        //user info
        public Guid UserId { get; set; }
        //other info
        public Institution Institution { get; set; }
        public Guid InstitutionUUID { get; set; }
        public AccreditationBody AccreditationBody { get; set; }
        public Guid AccreditationBodyUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
        //curriculum info
        public Curriculum Curriculum { get; set; }
        public Guid CurriculumUUID { get; set; }
        //Feedback
        public int StarRating { get; set; }
        public string Title { get; set; }
        public string FeedbackComment { get; set; }
    }


}
