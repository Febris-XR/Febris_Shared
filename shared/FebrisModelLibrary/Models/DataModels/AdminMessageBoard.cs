// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Interfaces.DataModelInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class AdminMessageBoard : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; } // lets use this to link? otherwise it is not stated as needed

        public string Subject { get; set; }
        public string Message { get; set; }
        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }


        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        public bool FromFebris { get; set; }

        public bool LimitToInstitutions { get; set; }
        public Institution Institution { get; set; }
        public Guid? InstitutionUUID { get; set; }

        public bool LimitToContentDevelopers { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid? ContentDeveloperUUID { get; set; }

        public bool LimitToAccreditationBodys { get; set; }
        public AccreditationBody AccreditationBody { get; set; }
        public Guid? AccreditationBodyUUID { get; set; }

        public bool Archive { get; set; }
    }
    
}
