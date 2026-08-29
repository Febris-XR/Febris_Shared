// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class InitalizationModels
    {
    }

    public class SimulationInitializerViewModel
    {
        //public Professional Professional { get; set; }
        public Guid ActorId { get; set; }
        public Module Module { get; set; }

        /// <summary>
        /// IGNORED BY THE NODE since ROADMAP 22. The record decision is derived server-side from
        /// the educator's per-cohort policy (LauncherLogic.ShouldRecordSession), because a launch
        /// request proves only the DEVICE and a client that could vote on its own recording could
        /// also dodge it. Retained on the wire contract because the central and developer tiers
        /// still bind this model; do not re-introduce a read of it on the node.
        /// </summary>
        public bool RecordSession { get; set; }
    }

}
