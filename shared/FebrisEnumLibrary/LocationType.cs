// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    public enum LocationType
    {
        [Display(Name = "Nursing School")] NursingSchool,
        [Display(Name = "Medical School")] MedicalSchool,

        [Display(Name = "Hospital")] Hospital,
        [Display(Name = "Training Center")] TrainingCenter,
        [Display(Name = "Urgent Care")] UrgentCare,
        [Display(Name = "Ambulatory Surgical Center")] AmbulatorySurgicalCenter,
        [Display(Name = "Birth Center")] BirthCenter,
        [Display(Name = "Blood Bank")] BloodBank,
        [Display(Name = "Dialysis Center")] DialysisCenter,
        [Display(Name = "Imaging or Radiology Center")] ImagingCenter,
        [Display(Name = "Clinic Or Medical Office")] ClinicOrMedicalOffice,
        [Display(Name = "Orthopedic Center")] OrthopedicCenter,

        [Display(Name = "Nursing Home")] NursingHome,
        [Display(Name = "Assisted Living Facility")] AssistedLivingFacility,
        [Display(Name = "Hospice Home")] HospiceHome,

        [Display(Name = "Correctional Facility")] CorrectionFacility,

        [Display(Name = "Dentistry Office or Clinic")] DentistryOfficeOrClinic,

        [Display(Name = "Option Not Listed")] Generic,
    }
}
