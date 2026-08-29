// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class FilterLinks
    {
    }
    public class CategoryLinkedFocus : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Focus Focus { get; set; }
        public Guid FocusUUID { get; set; }
        public Category Category { get; set; }
        public Guid CategoryUUID { get; set; }
    }
    public class IndustryLinkedCategory : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public Industry Industry { get; set; }
        public Guid IndustryUUID { get; set; }
        public Category Category { get; set; }
        public Guid CategoryUUID { get; set; }
    }


}
