// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    public class DashboardViewModel
    {
        public List<AdminMessageBoard> AdminMessageBoardList { get; set; }
        public List<MessageBoard> MessageBoardList { get; set; }
    }
}
