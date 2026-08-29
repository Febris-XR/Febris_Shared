// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;
using Febris.ModelLibrary.Models.MarketingModels;

namespace Febris.ModelLibrary.ViewModels
{
    public class PublicationCreationViewModel
    {
        public Publication Publication { get; set; }
        public Dictionary<string, string> Checksums { get; set; }
    }
}
