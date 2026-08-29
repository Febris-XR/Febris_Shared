// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Interfaces.XApiModelInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.XApiModels.ExtraModels
{
    public class XApiResultExtras: IXApiResultExtras
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public Result Result { get; set; }
        public Guid ResultUUID { get; set; }
        public int RestartCount { get; set; }
        public List<string> NotesList { get; set; }
    }
}
