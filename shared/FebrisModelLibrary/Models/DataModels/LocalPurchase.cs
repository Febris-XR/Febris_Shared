// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.LookupModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class LocalPurchase : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime Timestamp { get; set; }        
        ////
        public Guid ActorUUID { get; set; }        
        //public ProfessionalLinkedCurriculum ProfessionalLinkedCurriculum { get; set; }
        //public Guid ProfessionalLinkedCurriculumUUID { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public bool HasBeenProcessed { get; set; }
        public Guid CorrespondingUUID { get; set; }
    }
}
