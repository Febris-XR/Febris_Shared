// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    public class LeadTag : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ToolTip { get; set; }
    }
}
