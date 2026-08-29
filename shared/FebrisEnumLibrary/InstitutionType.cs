// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    //################################################################
    //################################################################
    public enum InstitutionType
    {        
        [Display(Name = "Hospital System")] HospitalSystem,
        [Display(Name = "Independent Hospital")] IndependentHospital,
        [Display(Name = "College or University")] School,
        [Display(Name = "Nurse Staffing Agency")] NurseStaffing,
        [Display(Name = "Palliative Care Provider")] PalliativeCareProvider,
        [Display(Name = "Hospice Care Provider")] HospiceCareProvider,
        [Display(Name = "Home Care Provider")] HomeCareProvider,
        [Display(Name = "Assisted Living Care Provider")] AssistedLivingCareProvider,
        [Display(Name = "Nursing Home Care Provider")] NursingHomeCareProvider,
        [Display(Name = "Correctional Facility Care Provider")] CorrectionalFacilityCareProvider,
        [Display(Name = "Dentistry Provider")] DentistryProvider,
    }
}
