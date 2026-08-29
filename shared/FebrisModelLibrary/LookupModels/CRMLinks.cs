// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models;
using Febris.ModelLibrary.Models.MarketingModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.LookupModels
{
    class CRMLinks
    {
    }

    public class LeadLinkedTag:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }

        public Lead Lead { get; set; }
        public Guid LeadUUID { get; set; }

        public LeadTag LeadTag { get; set; }
        public Guid LeadTagUUID { get; set; }
    }
}
