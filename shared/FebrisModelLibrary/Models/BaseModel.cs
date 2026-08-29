// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models
{
    public class BaseModel
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }

        [Display(Name = "Creation date")]
        public DateTime TimeStamp { get; set; }
        [Display(Name = "Last Modified")]
        public DateTime LastUpdateTimeStamp { get; set; }


    }
}
