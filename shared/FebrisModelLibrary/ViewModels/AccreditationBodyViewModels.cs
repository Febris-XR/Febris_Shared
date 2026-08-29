// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.UserModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class AccreditationBodyViewModels
    {
    }
    
    public class AccreditationBodyCreationViewModel
    {
        public AccreditationBody AccreditationBody { get; set; }
        //public AccreditationBodySettings AccreditationBodySettings { get; set; }
        public bool RemoveLogo { get; set; }
        // File-upload integrity check (passed to FebrisSecurityMethods.CheckSumValidation).
        public Dictionary<string, string> Checksums { get; set; }
    }
    public class AccreditationBodySelfEditViewModel
    {
        public AccreditationBody AccreditationBody { get; set; }
        public bool RemoveLogo { get; set; }
        public Dictionary<string, string> Checksums { get; set; }
    }
    




}
