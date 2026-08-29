// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Febris.ModelLibrary.Models.Scim
{
    /// <summary>
    /// SCIM 2.0 (RFC 7643 / 7644) wire DTOs for the inbound provisioning endpoint
    /// (SSO-M10). These are the JSON contract an IdP (Okta / Entra / ...) sends and
    /// receives; the mapping to <c>ApplicationUser</c> lives in the SSO BLL
    /// (<c>ScimUserMapper</c>). Property names follow the SCIM spec exactly, so the
    /// [JsonProperty] names are load-bearing.
    /// </summary>
    public static class ScimSchemas
    {
        public const string User = "urn:ietf:params:scim:schemas:core:2.0:User";
        public const string Group = "urn:ietf:params:scim:schemas:core:2.0:Group";
        public const string ListResponse = "urn:ietf:params:scim:api:messages:2.0:ListResponse";
        public const string Error = "urn:ietf:params:scim:api:messages:2.0:Error";
        public const string PatchOp = "urn:ietf:params:scim:api:messages:2.0:PatchOp";
    }

    /// <summary>SCIM complex "name" attribute.</summary>
    public class ScimName
    {
        [JsonProperty("givenName")] public string GivenName { get; set; }
        [JsonProperty("familyName")] public string FamilyName { get; set; }
        [JsonProperty("formatted")] public string Formatted { get; set; }
    }

    /// <summary>A SCIM multi-valued "emails" entry.</summary>
    public class ScimEmail
    {
        [JsonProperty("value")] public string Value { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("primary")] public bool Primary { get; set; }
    }

    /// <summary>SCIM common "meta" attribute (resource type + location + timestamps).</summary>
    public class ScimMeta
    {
        [JsonProperty("resourceType")] public string ResourceType { get; set; }
        [JsonProperty("created")] public DateTime? Created { get; set; }
        [JsonProperty("lastModified")] public DateTime? LastModified { get; set; }
        [JsonProperty("location")] public string Location { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
    }

    /// <summary>A SCIM core User resource.</summary>
    public class ScimUser
    {
        [JsonProperty("schemas")] public List<string> Schemas { get; set; } = new List<string> { ScimSchemas.User };
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("externalId")] public string ExternalId { get; set; }
        [JsonProperty("userName")] public string UserName { get; set; }
        [JsonProperty("name")] public ScimName Name { get; set; }
        [JsonProperty("displayName")] public string DisplayName { get; set; }
        [JsonProperty("emails")] public List<ScimEmail> Emails { get; set; }
        [JsonProperty("phoneNumbers")] public List<ScimEmail> PhoneNumbers { get; set; }
        [JsonProperty("active")] public bool Active { get; set; }
        [JsonProperty("meta")] public ScimMeta Meta { get; set; }
    }

    /// <summary>SCIM ListResponse envelope for query results.</summary>
    public class ScimListResponse<T>
    {
        [JsonProperty("schemas")] public List<string> Schemas { get; set; } = new List<string> { ScimSchemas.ListResponse };
        [JsonProperty("totalResults")] public int TotalResults { get; set; }
        [JsonProperty("startIndex")] public int StartIndex { get; set; }
        [JsonProperty("itemsPerPage")] public int ItemsPerPage { get; set; }
        // SCIM spells this member "Resources" (capital R) -- deliberate, per RFC 7644.
        [JsonProperty("Resources")] public List<T> Resources { get; set; } = new List<T>();
    }

    /// <summary>SCIM error envelope (RFC 7644 section 3.12).</summary>
    public class ScimError
    {
        [JsonProperty("schemas")] public List<string> Schemas { get; set; } = new List<string> { ScimSchemas.Error };
        /// <summary>HTTP status as a string, per the SCIM spec (e.g. "404").</summary>
        [JsonProperty("status")] public string Status { get; set; }
        /// <summary>Optional SCIM detail error keyword (e.g. "invalidFilter", "uniqueness").</summary>
        [JsonProperty("scimType", NullValueHandling = NullValueHandling.Ignore)] public string ScimType { get; set; }
        [JsonProperty("detail")] public string Detail { get; set; }
    }
}
