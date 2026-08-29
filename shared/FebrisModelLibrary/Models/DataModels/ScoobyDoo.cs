// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class ScoobyDoo:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Identification
        /// </summary>
        public License License { get; set; }
        public Guid LicenseUUID { get; set; }
        //public XApiModels.Object XApiObject { get; set; }
        public Guid XApiObjectUUID { get; set; }
        public Guid ActorUUID { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
        public Guid AccreditationBodyUUID { get; set; }
    }
}
