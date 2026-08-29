// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    public class LocalSoftwarePackageViewModel
    {
        public LocalSoftwarePackage LocalSoftwarePackage { get; set; }
        public Dictionary<string, string> Checksums { get; set; }
    }
}
