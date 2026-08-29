// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Interfaces.DataModelInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// This is the base of a single teaching module
    /// </summary>
    public class Module : BaseModel
    {
        //for db
        //public long Id { get; set; }
        //public Guid UUID { get; set; }


        //Out of date  
        [Display(Name = "This Module is Obsolete")]
        public bool Obsolete { get; set; }

        //Basic information
        //[Display(Name = "Creation date")]
        //public DateTime CreationTimeStamp { get; set; }

        //[Display(Name = "Last Modified")]
        //public DateTime UpdateTimeStamp { get; set; }



        [Display(Name = "Course Name")]
        public string Name { get; set; }

        public string Version { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public ModuleClassification ModuleClassification { get; set; }
        public Guid ModuleClassificationUUID { get; set; }

        /// <summary>
        /// new changes for marketplace
        /// 
        /// mayybe this should not be here. Could move this to its own lookup model
        /// </summary>        
        //public decimal Price { get; set; }
        //[Display(Name = "Education Category")]
        //public Industry Industry { get; set; }
        //[Display(Name = "Most Relevant Field")]
        //public Category Category { get; set; }        


        //catagorizing
        //[Display(Name = "Education Category")]
        //public EducationCategory EducationCategory { get; set; }
        //[Display(Name = "Most Relevant Field")]
        //public FieldType FieldType { get; set; }

        [Display(Name = "Pick this educaiton's main language (should be en-US)")]
        public LanguageMapTypeEnum Language { get; set; }

        [Display(Name = "Pick Interaction type (should be Performance for VR)")]
        public XApiInteractionType XApiInteractionType { get; set; }

        //step information
        [Display(Name = "Main section count")]
        public int MainSectionCount { get; set; }

        [Display(Name = "All sections and subsection count")]
        public int TotalSectionCount { get; set; }

        [Display(Name = "Solutions to test for steps to follow xAPI specificaiton")]
        public string InteractionComponents { get; set; }

        [Display(Name = "Estimated Completion Time in minutes")]
        public int EstimatedCompletionTime { get; set; }

        //test vs education
        //[Display(Name = "This listing is a Test and not Training.")]
        //public bool IsTest { get; set; }




    }
    public class ModuleClassification : BaseModel
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

    public class ModuleLinkedClassification : BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //[Display(Name = "Creation date")]
        //public DateTime CreationTimeStamp { get; set; }

        //[Display(Name = "Last Modified")]
        //public DateTime UpdateTimeStamp { get; set; }


        public ModuleClassification ModuleClassification { get; set; }
        public Guid ModuleClassificationUUID { get; set; }

        public Module Module { get; set; }
        public Guid ModuleUUID { get; set; }

    }
}
