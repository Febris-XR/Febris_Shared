// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    public class Publication : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        //public DateTime TimeStamp { get; set; }
        //public DateTime UpdateTimeStamp { get; set; }

        public bool Published { get; set; }
        public bool Archive { get; set; }

        public string LinkUrl { get; set; }

        public string Author { get; set; }
        public string Title { get; set; }
        public string MetaTitle { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }

        public string Image0 { get; set; }
        public string Image0Caption { get; set; }
        public string Image1 { get; set; }
        public string Image1Caption { get; set; }
        public string Image2 { get; set; }
        public string Image2Caption { get; set; }
        public string Image3 { get; set; }
        public string Image3Caption { get; set; }
        public string Image4 { get; set; }
        public string Image4Caption { get; set; }
        public string Video0 { get; set; }
        public string Video0Caption { get; set; }
        public string Video1 { get; set; }
        public string Video1Caption { get; set; }
        public string Video2 { get; set; }
        public string Video2Caption { get; set; }

    }
   
}
