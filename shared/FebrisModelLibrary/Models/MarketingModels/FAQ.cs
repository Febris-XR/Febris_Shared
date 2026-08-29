// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    public class FAQ : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }


        public bool Active { get; set; }

        public FAQCategory FAQCategory { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
