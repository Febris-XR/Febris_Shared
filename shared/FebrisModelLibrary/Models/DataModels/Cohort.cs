// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.UserModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    public class Cohort:BaseModel
    {
        public bool Archive { get; set; }
        public bool LockMembers { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// ROADMAP 22: the educator's recording policy for this cohort. When true, a simulation
        /// launched on this cohort's behalf is recorded, and the node attaches the video
        /// attachment that instructs the client to record.
        ///
        /// <para>
        /// The node DERIVES the launch's record decision from this flag rather than accepting a
        /// client-sent bool, because the only identity a launch request proves is the DEVICE (the
        /// hardware JWT) -- the ActorId on the request is client-asserted and validated against
        /// nothing, so a client that could vote on its own recording could opt out by asserting a
        /// different learner. Resolution is a UNION across the launch's two reachable cohort sets
        /// (the device's linked cohorts and the learner's memberships): if EITHER says record, the
        /// session records. See RecordingPolicyLogic.
        /// </para>
        ///
        /// <para>
        /// Defaults false, and the migration backfills existing rows to false: recording learner
        /// session video is opt-in per cohort, never something a schema change switches on.
        /// </para>
        /// </summary>
        public bool RecordSessions { get; set; }

        public Guid InstructorId { get; set; }

        //need to link to institution and potentially location
    }

    public class CohortMember : BaseModel
    {
        public Guid UserId { get; set; }

        public Cohort Cohort { get; set; }
        public Guid CohortUUID { get; set; }

    }

   
}
