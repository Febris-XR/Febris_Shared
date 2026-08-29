// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;

namespace Febris.ModelLibrary.Interfaces.DataModelInterfaces
{
    public interface IModule
    {
        long Id { get; set; }
        Guid UUID { get; set; }
        //Out of date  
        bool Obsolete { get; set; }
        //Basic information
        DateTime CreationDate { get; set; }        
        string Name { get; set; }
        string Version { get; set; }        
        string Description { get; set; }
        //catagorizing        
        //EducationCategory EducationCategory { get; set; }
        //FieldType FieldType { get; set; }
        //LanguageMapTypeEnum Language { get; set; }
        //XApiInteractionType XApiInteractionType { get; set; }
        //step information        
        int MainSectionCount { get; set; }
        int TotalSectionCount { get; set; }
        string InteractionComponents { get; set; }
        int EstimatedCompletionTime { get; set; }
        //test vs education
        bool IsTest { get; set; }
    }
}