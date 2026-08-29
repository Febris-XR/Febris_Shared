// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class ModuleLinks
    {
    }
    public class ModuleLinkedIndustry : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Module Module { get; set; }
        public Guid ModuleUUID { get; set; }
        public Industry Industry { get; set; }
        public Guid IndustryUUID { get; set; }
    }
    public class ModuleLinkedCategory : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Module Module { get; set; }
        public Guid ModuleUUID { get; set; }
        public Category Category { get; set; }
        public Guid CategoryUUID { get; set; }
    }
    public class ModuleLinkedFocus : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Module Module { get; set; }
        public Guid ModuleUUID { get; set; }
        public Focus Focus { get; set; }
        public Guid FocusUUID { get; set; }
    }
    public class ModuleLinkedTag : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }
        
        public Tag Tag { get; set; }
        public Guid TagUUID { get; set; }
        public Module Module { get; set; }
        public Guid ModuleUUID { get; set; }        
    }
    public class ModuleLinkedCurriculum : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Guid CurriculumUUID { get; set; }
        public Curriculum Curriculum { get; set; }
        public Guid ModuleUUID { get; set; }
        public Module Module { get; set; }
    }
    public class ModuleLinkedObject : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public long ObjectId { get; set; }
        public Guid ObjectUUID { get; set; }
        public Module Module { get; set; }
        public Guid ModuleUUID { get; set; }
    }
    public class ModuleHardwareCompatibility : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public XRHardwareModel XRHardwareModel { get; set; }
        public Guid XRHardwareModelUUID { get; set; }        
        public Module Module { get; set; }
        public Guid ModuleUUID { get; set; }        
    }
}
