// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.Models.UserModels;
using Febris.ModelLibrary.Models.XApiModels;
using Febris.ModelLibrary.Models.XApiModels.ExtraModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class XApiViewModels
    {
    }
    public class StatementDetailsViewModel
    {
        public Statement Statement { get; set; }
        //public Professional Professional { get; set; }
        //public ApplicationUser ApplicationUser { get; set; }
        public Module Module { get; set; }
        public string Video { get; set; }
        public XApiResultExtras xApiResultExtras { get; set; }
    }

    public class StatementInitializerGetViewModel
    {
        //public Professional Professional { get; set; }
        public Module ModuleBase { get; set; }
    }

    /// <summary>
    /// What a given DEVICE has submitted. The read side of
    /// <c>LocalStatement.SubmittedByHardwareUUID</c>.
    ///
    /// <para>
    /// The column exists so a forged learning record is "investigable instead of indistinguishable",
    /// and it originally shipped with two writers and no reader at all, which meant that promise
    /// could only be honoured with direct database access. This carries the answer to a Portal
    /// screen.
    /// </para>
    ///
    /// <para>
    /// CARRIES THE TOTAL SEPARATELY FROM THE ROWS on purpose. The row list is capped, and a screen
    /// that shows a truncated list without saying so tells an investigator they are looking at
    /// everything when they are not.
    /// </para>
    /// </summary>
    public class DeviceSubmissionSummaryViewModel
    {
        /// <summary>The device this summary describes.</summary>
        public Guid HardwareUUID { get; set; }

        /// <summary>Most recent first, capped at <see cref="Limit"/>.</summary>
        public List<Febris.ModelLibrary.Models.XApiModels.ModifiedForSharing.LocalStatement> Statements { get; set; }
            = new List<Febris.ModelLibrary.Models.XApiModels.ModifiedForSharing.LocalStatement>();

        /// <summary>Every statement this device submitted, not just the ones listed.</summary>
        public int TotalCount { get; set; }

        /// <summary>The cap that was applied, so a view can say "showing N of M" honestly.</summary>
        public int Limit { get; set; }

        /// <summary>True when the list is a truncated view of the total.</summary>
        public bool IsTruncated
        {
            get { return TotalCount > (Statements == null ? 0 : Statements.Count); }
        }

        /// <summary>
        /// How many DISTINCT learners this device submitted for. A shared classroom device is
        /// expected to show many, so this is context for the reader rather than a warning sign on
        /// its own.
        /// </summary>
        public int DistinctActorCount { get; set; }
    }
    public class StatementVoidingViewModel
    {
        public Statement Statement { get; set; }
        //public Professional Professional { get; set; }
        public Module Module { get; set; }
    }
    public class VerbCreationViewModel
    {
        public Verb Verb { get; set; }
        [Display(Name = "Select main display language")]
        public LanguageMapTypeEnum LanguageMap { get; set; }
        [Display(Name = "Description in main language")]
        public string Description { get; set; }

    }

    public class StatementDataViewModel
    {
        public Statement Statement { get; set; }
        public XApiResultExtras XApiResultExtras { get; set; }
    }

    public class XApiResultExtrasViewModel
    {
        public RadarChart RadarChart { get; set; }
        public XApiResultExtras XApiResultExtras { get; set; }
    }

}
