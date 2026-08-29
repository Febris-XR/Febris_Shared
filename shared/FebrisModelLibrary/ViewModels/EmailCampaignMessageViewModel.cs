// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.MarketingModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    public class EmailCampaignMessageViewModel
    {
        public long Id { get; set; }

        public Guid? CampaignUUID { get; set; }
        public Campaign Campaign { get; set; }

        public EmailCampaignMessage EmailCampaignMessage { get; set; }
        public EmailSectionViewModel EmailSectionViewModel { get; set; }
    }
}

