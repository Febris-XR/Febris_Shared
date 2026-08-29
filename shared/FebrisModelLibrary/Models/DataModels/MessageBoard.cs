// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Interfaces.DataModelInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class MessageBoard:BaseModel
    {
        public bool Archive { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }

        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        /// <summary>
        /// I think these should be broken out
        /// </summary>
        ///         

        public Institution Institution { get; set; }
        public Guid? InstitutionUUID { get; set; }
        
        public ContentDeveloper ContentDeveloper { get; set; }
        public Guid? ContentDeveloperUUID { get; set; }

        public AccreditationBody AccreditationBody { get; set; }
        public Guid? AccreditationBodyUUID { get; set; }

        public Location Location { get; set; }
        public Guid? LocationUUID { get; set; }
    }
}
