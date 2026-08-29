// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Interfaces.DataModelInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class DailyUse :BaseModel
    {       
        public DateTime Date { get; set; }
        public AccountType TenantType { get; set; }
        public InstitutionType InstitutionType { get; set; }
        public int TrainingModuleTotal { get; set; }
        public int TestingModuleTotal { get; set; }
        public double TrainingTimeDuration { get; set; }
        public double TestingTimeDuration { get; set; }
        public long VideoByteSize { get; set; }
        public ContentDeveloper ContentDeveloper { get; set; }
        public long ContentDeveloperId { get; set; }
        public Guid ContentDeveloperUUID { get; set; }
        public Institution Institution { get; set; }
        public long InstitutionId { get; set; }
        public Guid InstitutionUUID { get; set; }
    }
}
