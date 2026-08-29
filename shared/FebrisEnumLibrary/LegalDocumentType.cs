// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.EnumLibrary
{
    public enum LegalDocumentType
    {
        None = 0,
        Generic = 101,
        [Display(Name = "Liability Waiver")] LiabilityWaiver = 415,
        [Display(Name = "License Agreement")] LicenseAgreement = 453,
        [Display(Name = "Service Agreement")] ServiceAgreement = 786,
        [Display(Name = "EULA")] EULA = 981,
        [Display(Name = "Terms Of Use")] TermsOfUse =850,
        [Display(Name = "Privacy Policy")] PrivacyPolicy = 753,

    }
}
