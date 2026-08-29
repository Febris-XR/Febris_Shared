// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Febris.ModelLibrary.Models.Scim
{
    /// <summary>A SCIM PatchOp request (RFC 7644 section 3.5.2).</summary>
    public class ScimPatchOp
    {
        [JsonProperty("schemas")] public List<string> Schemas { get; set; }
        [JsonProperty("Operations")] public List<ScimPatchOperation> Operations { get; set; } = new List<ScimPatchOperation>();
    }

    /// <summary>One operation within a SCIM PatchOp.</summary>
    public class ScimPatchOperation
    {
        /// <summary>add | replace | remove (case-insensitive).</summary>
        [JsonProperty("op")] public string Op { get; set; }
        /// <summary>Optional attribute path (e.g. "active", "name.givenName"); when absent, <see cref="Value"/> is an object of attributes.</summary>
        [JsonProperty("path")] public string Path { get; set; }
        /// <summary>Scalar (with a path) or an object of attributes (without a path). Kept as a JToken since SCIM values are polymorphic.</summary>
        [JsonProperty("value")] public JToken Value { get; set; }
    }
}
