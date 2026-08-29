// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// Used for filtering in marketplace
    /// </summary>
    public class Tag : BaseModel
    {
        //for db
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; }
        public TagType TagType { get; set; }
        public string Description { get; set; }
    }
}
