// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class MessageBoardViewModels
    {
    }
    public class AdminMessageBoardCreationViewModel
    {

        public AdminMessageBoard AdminMessageBoard { get; set; }
        public SelectList InstitutionSelectList { get; set; }
        public long? SelectedInstitutionId { get; set; }
        public SelectList ContentDeveloperSelectList { get; set; }
        public long? SelectedContentDeveloperId { get; set; }
        public SelectList AccreditationBodySelectList { get; set; }
        public long? SelectedAccreditationBodyId { get; set; }
    }
}
