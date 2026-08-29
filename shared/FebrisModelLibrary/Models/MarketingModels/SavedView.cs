// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// CRM Phase 1 (2026-05-20): user-scoped saved filter/segment for
    /// the Lead / Account / Task list views.
    /// <para>
    /// <see cref="FilterJson"/> is intentionally a free-form string at
    /// Phase 1. The shape is owned by the consuming list view (e.g.,
    /// Lead Index serializes its filter state to JSON and stores the
    /// whole blob here). When the AdminPortal lead-filter UI lands
    /// (Phase 1.5), it will define the canonical schema; the persistence
    /// layer doesn't need to know.
    /// </para>
    /// <para>
    /// <see cref="UserId"/> scopes the view to the creating user --
    /// "My Healthcare leads" is personal. Shared/team views are a
    /// Phase 1.5 enhancement (add IsShared flag + a Team scope, or a
    /// separate SharedView table). For now, all views are per-user.
    /// </para>
    /// </summary>
    public class SavedView : BaseModel
    {
        /// <summary>ApplicationUser.Id of the rep who owns this view.</summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>Human-readable label shown in the "Load view" dropdown.</summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Which entity list this view applies to. Free string in Phase 1
        /// ("Lead" / "Account" / "Task"); promote to an enum if/when
        /// other entities adopt saved views.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string EntityType { get; set; }

        /// <summary>
        /// Serialized filter state. Schema owned by the consuming list
        /// view -- see the class XML doc.
        /// </summary>
        [Required]
        public string FilterJson { get; set; }
    }
}
