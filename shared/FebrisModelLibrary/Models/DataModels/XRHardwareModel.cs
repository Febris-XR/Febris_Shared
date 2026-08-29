// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// This is for specific hardware such as Oculus Quest 2, Vive Focus 3 etc.
    /// This is for filtering out compatibility
    /// So this is more like a tag for modules
    /// </summary>
    public class XRHardwareModel : BaseModel
    {        
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
                
        public XRHardwareClass XRHardwareClass { get; set; }        
    }
}
