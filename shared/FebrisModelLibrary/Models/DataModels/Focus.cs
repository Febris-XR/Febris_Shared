// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// 
    /// Industry->Field->Category
    /// </summary>
    public class Focus : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
