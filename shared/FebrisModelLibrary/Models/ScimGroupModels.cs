// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Febris.ModelLibrary.Models.Scim
{
    /// <summary>A member reference inside a SCIM Group (RFC 7643 section 4.2).</summary>
    public class ScimGroupMember
    {
        /// <summary>The member's resource id (an ApplicationUser id).</summary>
        [JsonProperty("value")] public string Value { get; set; }
        [JsonProperty("display")] public string Display { get; set; }
        [JsonProperty("$ref")] public string Ref { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
    }

    /// <summary>
    /// A SCIM core Group resource. In this platform a Group IS a role:
    /// id/displayName are the role name and members are the
    /// institution's users holding that role.
    /// </summary>
    public class ScimGroup
    {
        [JsonProperty("schemas")] public List<string> Schemas { get; set; } = new List<string> { ScimSchemas.Group };
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("displayName")] public string DisplayName { get; set; }
        [JsonProperty("members")] public List<ScimGroupMember> Members { get; set; } = new List<ScimGroupMember>();
        [JsonProperty("meta")] public ScimMeta Meta { get; set; }
    }
}
