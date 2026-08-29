// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;

namespace Febris.ModelLibrary.Models.DataModels
{
    // Links a parent/guardian account (InstitutionUserAccountType.UserParent) to a
    // student so the parent can be granted FERPA-compliant read-only access to that
    // student's learning records. One row per parent-student link:
    //   - a parent can have many students (many rows with the same ParentUserId),
    //   - a student can have several guardians (many rows with the same StudentActorId).
    // StudentActorId is denormalized from the student's LocalApplicationUser.Actor so
    // access scoping resolves the accessible actor set without a second user lookup.
    // BaseModel supplies Id, UUID, TimeStamp, LastUpdateTimeStamp for link-level audit.
    public class ParentLinkedStudent : BaseModel
    {
        public Guid ParentUserId { get; set; }
        public Guid StudentUserId { get; set; }
        public Guid StudentActorId { get; set; }
    }
}
