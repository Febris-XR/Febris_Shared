// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// This is the actual curriculum for a list of modules
    /// ie. CNA course, RN on-boarding
    /// </summary>
    public class Curriculum : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }

        ///// <summary>
        ///// Generic Data
        ///// </summary>        
        //[Display(Name = "Creation Date")]
        //public DateTime CreationTimeStamp { get; set; }
        //[Display(Name = "Last Modification Date")]
        //public DateTime UpdateTimeStamp { get; set; }
        [Display(Name = "This listing is Obsolete")]
        public bool Obsolete { get; set; }

        public CurriculumClassification CurriculumClassification { get; set; }
        public Guid? CurriculumClassificationUUID { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }



        ///// <summary>
        ///// Curriculum Price
        ///// </summary>
        //[Display(Name = "Price in US Dollars")]
        //public decimal Price { get; set; }

    }

    public class CurriculumClassification : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //[Display(Name = "Creation date")]
        //public DateTime CreationTimeStamp { get; set; }

        //[Display(Name = "Last Modified")]
        //public DateTime UpdateTimeStamp { get; set; }

        public bool Obsolete { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    //public class CurriculumLinkedClassification
    //{
    //    public long Id { get; set; }
    //    public Guid UUID { get; set; }
    //    [Display(Name = "Creation date")]
    //    public DateTime CreationTimeStamp { get; set; }

    //    [Display(Name = "Last Modified")]
    //    public DateTime UpdateTimeStamp { get; set; }


    //    public CurriculumClassification CurriculumClassification { get; set; }
    //    public Guid CurriculumClassificationUUID { get; set; }

    //    public Curriculum Curriculum { get; set; }
    //    public Guid CurriculumUUID { get; set; }

    //}

}
