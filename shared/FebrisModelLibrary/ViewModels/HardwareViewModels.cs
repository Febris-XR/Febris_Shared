// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.LookupModels;
using Febris.ModelLibrary.Models.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class HardwareViewModels { }
    public class HardwareViewModel
    {        
        public bool IsLockedOut { get; set; }
                
        public Hardware Hardware { get; set; }
        public List<Location> LocationList { get; set; }
        public List<Module> ModuleList { get; set; }
        public List<Curriculum> CurriculumList { get; set; }
        //public string LocationName { get; set; }
        //public string HardwareName { get; set; }
        //public string ModuleName { get; set; }
        //public string CurriculumName { get; set; }
    }
        
    public class HardwareCreationViewModel
    {
        public Hardware Hardware { get; set; }
        [Display(Name = "Select the type of hardware")]
        public SelectList HardwareTypeSelectList { get; set; }
        public Guid SelectedHardwareType { get; set; }
    }

    /// <summary>
    /// The NODE's hardware registration view model. Twin of
    /// <see cref="HardwareCreationViewModel"/>, which stays exactly as it is for the central and
    /// developer portals.
    ///
    /// <para>
    /// WHY A TWIN. The shared model above is typed to the CENTRAL <see cref="Hardware"/> aggregate,
    /// so the node's own registration screens were projecting node devices through a type the node
    /// does not own, has no writer for, and is trying to shed. That is the same leak that put the
    /// central entity into the node's EF model and produced the orphan Hardware1 table. Naming
    /// precedent is <c>LocalHardwareLinkedModuleViewModel</c> and
    /// <c>LocalHardwareLinkedModule</c>: the node gets a twin, the shared type is left alone.
    /// </para>
    ///
    /// <para>
    /// There is no select list and no selected-UUID field. The node binds
    /// <c>LocalHardware.HardwareKind</c> directly through <c>Html.GetEnumSelectList</c>, so the
    /// dropdown no longer depends on the lookup table being seeded and correctly populated. The
    /// lookup is consulted only to STAMP the inert hub-reconciliation carriers after the operator
    /// has chosen a kind.
    /// </para>
    /// </summary>
    public class LocalHardwareCreationViewModel
    {
        public LocalHardware Hardware { get; set; }
    }

    /// <summary>
    /// A device plus what it has SUBMITTED, for the device detail screen.
    ///
    /// <para>
    /// The two halves come from two SEPARATE DATABASES: the device row from DataDb and the
    /// statements from XApiDb. They are composed here rather than joined, because this node keeps
    /// those databases physically apart and no cross-database transaction exists. That also means
    /// the two halves are read at slightly different moments, which is fine for an attribution
    /// screen and would not be for anything enforcing a rule.
    /// </para>
    ///
    /// <para>
    /// <see cref="Submissions"/> is never null once the controller has run, so the view can render
    /// the panel without null-checking every field. A device that has submitted nothing yields an
    /// empty summary, which is a meaningful answer rather than a missing one.
    /// </para>
    /// </summary>
    public class LocalHardwareDetailsViewModel
    {
        public LocalHardware Hardware { get; set; }

        public DeviceSubmissionSummaryViewModel Submissions { get; set; }
            = new DeviceSubmissionSummaryViewModel();

        /// <summary>
        /// Video this device minted. Null when the recording store could not be read, which the
        /// view distinguishes from "none", because those mean different things during an incident.
        /// </summary>
        public DeviceRecordingSummaryViewModel Recordings { get; set; }
            = new DeviceRecordingSummaryViewModel();
    }

    /// <summary>
    /// Video recordings minted by one device, for the device detail screen.
    ///
    /// <para>
    /// AN OPERATIONS VIEW, NOT A SECURITY CONTROL. "What has this device done" is a routine admin
    /// question: a headset attributing to the wrong roster, a device that has stopped recording, a
    /// support call about a missing video, a device being retired. Nothing here refuses, hides or
    /// flags anything.
    /// </para>
    ///
    /// <para>
    /// A recording carries the LEARNER, which arrives from the launch context, and the DEVICE, which
    /// the device token proves. That split is the shared-kiosk design and not a weakness in it: a
    /// classroom device cannot prove which learner is standing at it, and one headset serves a whole
    /// class in sequence. Several learners on one device is ordinary. See the ownership ruling in
    /// <c>docs/BUGS.md</c>, which also retracts an earlier write-up that treated the split as a
    /// defect.
    /// </para>
    /// </summary>
    public class DeviceRecordingSummaryViewModel
    {
        public Guid HardwareUUID { get; set; }

        /// <summary>Most recent first, capped at <see cref="Limit"/>.</summary>
        public List<Recording> Recordings { get; set; } = new List<Recording>();

        /// <summary>Every recording this device minted, not just the ones listed.</summary>
        public int TotalCount { get; set; }

        public int Limit { get; set; }

        public bool IsTruncated
        {
            get { return TotalCount > (Recordings == null ? 0 : Recordings.Count); }
        }

        /// <summary>
        /// How many DISTINCT learners this device minted video for, on this page. A shared device is
        /// expected to show several, so this is context for a reader rather than a warning.
        /// </summary>
        public int DistinctActorCount { get; set; }
    }



}
