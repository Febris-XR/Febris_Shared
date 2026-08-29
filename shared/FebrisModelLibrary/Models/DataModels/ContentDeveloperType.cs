// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class ContentDeveloperType : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }

        //[Display(Name = "Can Make Accrediting Body")]
        //public bool CanMakeAccreditingBody { get; set; }
        //[Display(Name = "Can Connect To Accrediting Body")]
        //public bool CanConnectToAccreditingBody { get; set; }

        //This is needed but It needs to have some type of control

    }
}