// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.DataModels
{
    /// <summary>
    /// The node's LOCAL single-tenant identity: InstitutionSettings identity is rewired off the
    /// hub-issued License claim onto a local single-tenant identity.
    ///
    /// <para>
    /// Historically the tenant tier learned "who am I" from the scheme-B License claim
    /// (<c>License.Institution</c>, hub-issued): the shared
    /// <c>InstitutionSettingsLogic.GetSettings()</c> derives the settings id from that claim, and
    /// the Institution/InstitutionSettings Remote reads fetched the rest from central. A
    /// self-sufficient node has no license, so it owns this row instead: exactly ONE per
    /// deployment, seeded idempotently at provision time by <c>NodeIdentitySeeder</c> in the
    /// tenant's own DataDb. <see cref="InstitutionUUID"/> is generated once at provision and
    /// persisted -- it is the node's stable institution identity, the value license-claim-derived
    /// reads fall back to when no license is present. When a hub IS attached later, the hub's
    /// institution registry entry can adopt or map this UUID; nothing about local operation
    /// changes.
    /// </para>
    /// </summary>
    public class NodeIdentity : BaseModel
    {
        /// <summary>Human-readable deployment name (shown where Institution.Name was shown).
        /// Seeded from config <c>NodeIdentity:Name</c> when present, else a generic default.</summary>
        [Display(Name = "Node Name")]
        public string Name { get; set; }

        /// <summary>The node's stable institution identity, generated at provision time and
        /// persisted. Fills the role the License claim's <c>InstitutionUUID</c> played on
        /// hub-coupled deployments.</summary>
        [Display(Name = "Institution UUID")]
        public Guid InstitutionUUID { get; set; }
    }
}
