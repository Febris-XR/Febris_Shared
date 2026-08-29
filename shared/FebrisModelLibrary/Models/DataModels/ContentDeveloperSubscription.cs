// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// Content Developer Subscription setup and handling
    /// </summary>
    public class ContentDeveloperSubscription : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }        

        ///// <summary>
        ///// Generic Data
        ///// </summary>
        //public DateTime CreationTimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        /// <summary>
        /// Links
        /// </summary>
        public long ContentDeveloperId { get; set; }
        public Guid? ContentDeveloperUUID { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public License License { get; set; }
        public Guid? LicenseUUID { get; set; }

        /// <summary>
        /// Subscription data
        /// </summary>
        public DeploymentType DeploymentType { get; set; }
        public bool IsActive { get; set; }
        public string ActiveToken { get; set; }
    }
}
