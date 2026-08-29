// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels
{
    /// <summary>
    /// One parent-to-student link, flattened for the admin link-management UI.
    /// Pairs the stored link keys (which the unlink action needs) with the
    /// student's display details (resolved from the student's Identity account)
    /// so the view can show who is linked without a second lookup.
    /// </summary>
    public class ParentLinkViewModel
    {
        /// <summary>The parent/guardian user this link belongs to.</summary>
        public Guid ParentUserId { get; set; }

        /// <summary>The linked student's Identity user id.</summary>
        public Guid StudentUserId { get; set; }

        /// <summary>
        /// The linked student's xApi Actor UUID. This is the key the unlink action
        /// uses, and the value XApiAccessScope grants the parent read access to.
        /// </summary>
        public Guid StudentActorId { get; set; }

        /// <summary>Student display name, for the admin UI.</summary>
        public string StudentName { get; set; }

        /// <summary>Student email, for the admin UI.</summary>
        public string StudentEmail { get; set; }
    }

    /// <summary>
    /// Backs the admin "manage a parent's links" page: the parent being managed,
    /// the students already linked to them, and the students available to link.
    /// </summary>
    public class ParentLinkManagementViewModel
    {
        /// <summary>The parent/guardian account being managed.</summary>
        public Guid ParentUserId { get; set; }

        /// <summary>Students already linked to this parent.</summary>
        public List<ParentLinkViewModel> LinkedStudents { get; set; } = new List<ParentLinkViewModel>();

        /// <summary>Learner accounts in the tenant that an admin can link.</summary>
        public List<LocalUserViewModel> LinkableStudents { get; set; } = new List<LocalUserViewModel>();
    }
}
