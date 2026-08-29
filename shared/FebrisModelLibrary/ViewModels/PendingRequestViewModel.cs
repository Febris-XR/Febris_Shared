// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.LookupModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    public class PendingRequestViewModel
    {
        public List<InstitutionLinkedUser> InstitutionLinkedUserList { get; set; }

        public List<LocationLinkedUser> LocationLinkedUserList { get; set; }

        public List<InstitutionLinkedProfessional> InstitutionLinkedProfessionalList { get; set; }
    }
}
