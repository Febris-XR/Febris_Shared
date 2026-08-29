// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    public enum FieldType
    {
        [Display(Name = "Allergology")] Allergology,
        [Display(Name = "Anesthesiology")] Anesthesiology,
        [Display(Name = "Cardiology")] Cardiology,
        [Display(Name = "Clinical Pharmacy")] ClinicalPharmacy,
        [Display(Name = "Dermatology")] Dermatology,
        [Display(Name = "Diabetic Foot Care")] DiabeticFootCare,
        [Display(Name = "Diagnostic Imaging")] DiagnosticImaging,
        [Display(Name = "Emergency Medicine")] EmergencyMedicine,
        [Display(Name = "Endodontics")] Endodontics,
        [Display(Name = "Ergonomics")] Ergonomics,
        [Display(Name = "Family and General Practice")] FamilyAndGeneralPractice,
        [Display(Name = "Gastroenterology")] Gastroenterology,
        [Display(Name = "Geriatric Optometry")] GeriatricOptometry,
        [Display(Name = "Geriatric Pharmacy")] GeriatricPharmacy,
        [Display(Name = "Geriatrics")] Geriatrics,
        [Display(Name = "Hospital-Based Optometry")] HospitalBasedOptometry,
        [Display(Name = "Infectious Disease")] InfectiousDisease,
        [Display(Name = "Internal Disorders")] InternalDisorders,
        [Display(Name = "Internal Medicine")] InternalMedicine,
        [Display(Name = "Intravenous Nutrition Support")] IntravenousNutritionSupport,
        [Display(Name = "Neurology")] Neurology,
        [Display(Name = "Nuclear Pharmacy")] NuclearPharmacy,
        [Display(Name = "Nutrition")] Nutrition,
        [Display(Name = "Obstetrics & Gynecology")] ObstetricsAndGynecology,
        [Display(Name = "Ocular Disease")] OcularDisease,
        [Display(Name = "Oncology")] Oncology,
        [Display(Name = "Oral & Maxillofacial Radiology")] OralAndMaxillofacialRadiology,
        [Display(Name = "Oral & Maxillofacial Surgery")] OralAndMaxillofacialSurgery,
        [Display(Name = "Oral Pathology")] OralPathology,
        [Display(Name = "Orthodontics")] Orthodontics,
        [Display(Name = "Orthopedics")] Orthopedics,
        [Display(Name = "Pathology")] Pathology,
        [Display(Name = "Pediatric Dentistry")] PediatricDentistry,
        [Display(Name = "Pediatric Optometry")] PediatricOptometry,
        [Display(Name = "Pediatrics")] Pediatrics,
        [Display(Name = "Periodontics")] Periodontics,
        [Display(Name = "Primary Care Optometry")] PrimaryCareOptometry,
        [Display(Name = "Prosthodontics")] Prosthodontics,
        [Display(Name = "Psychiatry")] Psychiatry,
        [Display(Name = "Psychopharmacotherapy")] Psychopharmacotherapy,
        [Display(Name = "Public Health")] PublicHealth,
        [Display(Name = "Radiology")] Radiology,
        [Display(Name = "Research")] Research,
        [Display(Name = "Sports Injuries")] SportsInjuries,
        [Display(Name = "Surgery")] Surgery,
        [Display(Name = "Vision Therapy")] VisionTherapy,
        [Display(Name = "Hospice Care")] HospiceCare,
        [Display(Name = "Palliative Care")] PalliativeCare,

    }
}
