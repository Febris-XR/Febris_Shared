// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels.XApi
{
    // =====================================================================
    // xAPI 1.0.3 statement DTOs -- wire-format mirror.
    // =====================================================================
    //
    // These shapes mirror the JSON xAPI producers (PC launcher, mobile
    // companion, simulation plugins) POST to /api/Statement/Backup.
    // They are DISTINCT from the persisted-domain models under
    // Models/XApiModels/ -- the persisted types have DB ids, BaseModel
    // inheritance, and EF navigation properties; the DTOs here have only
    // the fields xAPI defines on the wire.
    //
    // <b>Vendor extensions:</b> every DTO carries a [JsonExtensionData]
    // catchall so non-spec fields a producer emits round-trip cleanly to
    // the persisted JSON form. This is part of the raw-byte preservation
    // strategy for statement ingest -- the binder ALSO captures the raw
    // POST body, but the DTO's
    // extension data is a defense-in-depth so unknown fields aren't lost
    // even when downstream code consumes only the typed form.
    //
    // <b>Pairing with raw bytes:</b> the controller doesn't take this DTO
    // alone -- it takes <see cref="XApiStatementSubmission"/> which bundles
    // the parsed DTO with the verbatim request body. Both flow through to
    // the BLL and persistence. See <c>XApiRawBodyModelBinder</c> in the
    // SSO API for the binder implementation.
    //
    // Spec: https://github.com/adlnet/xAPI-Spec/blob/1.0.3/xAPI-Data.md

    /// <summary>
    /// xAPI 1.0.3 Statement (spec section 4.1).
    /// </summary>
    public class XApiStatementDto
    {
        /// <summary>Statement UUID. Optional; producer-assigned. Server stamps one if missing.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Required. Who performed the action.</summary>
        [JsonProperty("actor")]
        public XApiActorDto Actor { get; set; }

        /// <summary>Required. What action was performed.</summary>
        [JsonProperty("verb")]
        public XApiVerbDto Verb { get; set; }

        /// <summary>Required. What the action was performed on.</summary>
        [JsonProperty("object")]
        public XApiObjectDto Object { get; set; }

        /// <summary>Optional. Outcome (score, completion, etc.).</summary>
        [JsonProperty("result")]
        public XApiResultDto Result { get; set; }

        /// <summary>Optional. Surrounding circumstance (registration, instructor, context activities, etc.).</summary>
        [JsonProperty("context")]
        public XApiContextDto Context { get; set; }

        /// <summary>Optional ISO 8601 timestamp of when the statement event occurred.</summary>
        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; }

        /// <summary>Server-assigned ISO 8601 timestamp of when the statement was stored.</summary>
        [JsonProperty("stored")]
        public DateTime? Stored { get; set; }

        /// <summary>Optional. The entity asserting the statement is true (often the LRS itself).</summary>
        [JsonProperty("authority")]
        public XApiAuthorityDto Authority { get; set; }

        /// <summary>Optional. xAPI version string, e.g. "1.0.3".</summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>Optional. Auxiliary files referenced by the statement.</summary>
        [JsonProperty("attachments")]
        public List<XApiAttachmentDto> Attachments { get; set; }

        /// <summary>
        /// Catchall for any fields the producer sent that aren't modeled
        /// above (vendor-specific extensions on the statement element
        /// itself, future spec additions, etc.). Newtonsoft populates this
        /// during deserialization and round-trips it on serialization.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>
    /// xAPI Actor (spec section 4.1.2). Used for the statement Actor, Authority,
    /// nested Instructor/Team, members of a Group, and Agent/Group when
    /// they appear as a statement Object.
    /// </summary>
    public class XApiActorDto
    {
        /// <summary>"Agent" (default) or "Group".</summary>
        [JsonProperty("objectType")]
        public string ObjectType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        // ---- Inverse Functional Identifier (IFI) ----
        // For an Agent: EXACTLY ONE of mbox / mbox_sha1sum / openid / account.
        // For a Group: the IFI is optional (anonymous Group), but if present
        // the same rule applies.

        /// <summary>mailto: URI.</summary>
        [JsonProperty("mbox")]
        public string Mbox { get; set; }

        /// <summary>Hex SHA-1 of mailto: URI.</summary>
        [JsonProperty("mbox_sha1sum")]
        public string MboxSha1Sum { get; set; }

        /// <summary>OpenID URI.</summary>
        [JsonProperty("openid")]
        public string OpenId { get; set; }

        [JsonProperty("account")]
        public XApiAccountDto Account { get; set; }

        /// <summary>
        /// Group only. Anonymous-Group has members; identified-Group MAY have them.
        /// <para>
        /// Tolerant binding (SDKV-14): the Febris wire dialect carries member as a
        /// wrapper OBJECT <c>{id, uuid, actors:[...]}</c>, the xAPI 1.0.3 spec as a
        /// bare array of Agents. <see cref="XApiTolerantActorListConverter"/> accepts
        /// both (plus null), binding into this list shape.
        /// </para>
        /// </summary>
        [JsonProperty("member")]
        [JsonConverter(typeof(XApiTolerantActorListConverter))]
        public List<XApiActorDto> Member { get; set; }

        // ---- Febris-internal DB lookup hints (NOT in xAPI 1.0.3 spec) ----
        // External xAPI producers omit these; Febris-internal services
        // (PC launcher, MDM, simulation plugins) stamp them on so the
        // typed factor can skip the IFI lookup and hit the DB primary
        // key directly. Round-trip-safe through serialization. Honored
        // when present; ignored when null. Mirrors the lookup priority
        // the JObject factor uses at StatementFactor.SetupActor lines
        // 178-215.

        /// <summary>Febris-internal Actor.Id (numeric DB PK). Skip IFI lookup if set.</summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        /// <summary>Febris-internal Actor.UUID. Second-priority lookup after Id.</summary>
        [JsonProperty("uuid", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? UUID { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>xAPI Account-style IFI (spec section 4.1.2.1).</summary>
    public class XApiAccountDto
    {
        [JsonProperty("homePage")]
        public string HomePage { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>xAPI Verb (spec section 4.1.3).</summary>
    public class XApiVerbDto
    {
        /// <summary>Required. Verb IRI (e.g. "http://adlnet.gov/expapi/verbs/completed").</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Optional LanguageMap (e.g. {"en-US": "completed"}).</summary>
        [JsonProperty("display")]
        public IDictionary<string, string> Display { get; set; }

        // ---- Febris-internal DB lookup hints (NOT in xAPI 1.0.3 spec) ----
        // xAPI Verb.Id is a URI; Febris persists Verbs with a numeric DB
        // PK named "Key" alongside the URI. Internal services stamp
        // Key + UUID into the wire so the typed factor can hit the DB
        // by PK instead of by URI. Honored when present; ignored when
        // null. Mirrors lookup at StatementFactor.SetupVerb lines 488-514.

        /// <summary>Febris-internal Verb.Key (numeric DB PK).</summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public long? Key { get; set; }

        /// <summary>Febris-internal Verb.UUID.</summary>
        [JsonProperty("uuid", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? UUID { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>
    /// xAPI Object (spec section 4.1.4). objectType discriminates between Activity
    /// (the most common), Agent / Group (the Actor shape), SubStatement
    /// (a full Statement nested), and StatementRef (a reference to another
    /// statement by UUID).
    /// </summary>
    public class XApiObjectDto
    {
        /// <summary>"Activity" (default) | "Agent" | "Group" | "SubStatement" | "StatementRef".</summary>
        [JsonProperty("objectType")]
        public string ObjectType { get; set; }

        /// <summary>Activity IRI -- or StatementRef UUID.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Activity definition (Activity objectType only).</summary>
        [JsonProperty("definition")]
        public XApiActivityDefinitionDto Definition { get; set; }

        // ---- Agent / Group fields (when objectType is Agent or Group) ----
        // Duplicated from XApiActorDto since C# can't easily express
        // a sum-type. JsonExtensionData below catches anything we miss.

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mbox")]
        public string Mbox { get; set; }

        [JsonProperty("mbox_sha1sum")]
        public string MboxSha1Sum { get; set; }

        [JsonProperty("openid")]
        public string OpenId { get; set; }

        [JsonProperty("account")]
        public XApiAccountDto Account { get; set; }

        /// <summary>Tolerant binding (SDKV-14): accepts the dialect wrapper object or the spec array. See <see cref="XApiActorDto.Member"/>.</summary>
        [JsonProperty("member")]
        [JsonConverter(typeof(XApiTolerantActorListConverter))]
        public List<XApiActorDto> Member { get; set; }

        // ---- SubStatement fields (when objectType is SubStatement) ----
        // SubStatement carries an entire Statement minus id/stored/authority/
        // version/attachments. We model only the cross-cutting fields here
        // and let ExtensionData carry anything additional. Round-tripping
        // SubStatement is non-trivial; producers using it are rare.

        [JsonProperty("actor")]
        public XApiActorDto Actor { get; set; }

        [JsonProperty("verb")]
        public XApiVerbDto Verb { get; set; }

        [JsonProperty("object")]
        public XApiObjectDto NestedObject { get; set; }

        [JsonProperty("result")]
        public XApiResultDto Result { get; set; }

        [JsonProperty("context")]
        public XApiContextDto Context { get; set; }

        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; }

        // ---- Febris-internal DB lookup hints (NOT in xAPI 1.0.3 spec) ----
        // xAPI Object.Id is a URI (for Activity objectType) or a UUID-
        // shaped string (for StatementRef). Febris persists Activities
        // with a numeric DB PK named "Key" alongside the URI. Internal
        // services stamp Key + UUID for fast DB lookup. Honored when
        // present; ignored when null. Mirrors lookup at StatementFactor.
        // SetupObject lines 351-378.

        /// <summary>Febris-internal Object.Key (numeric DB PK).</summary>
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public long? Key { get; set; }

        /// <summary>Febris-internal Object.UUID. Second-priority lookup after Key.</summary>
        [JsonProperty("uuid", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? UUID { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>xAPI Activity Definition (spec section 4.1.4.1).</summary>
    public class XApiActivityDefinitionDto
    {
        [JsonProperty("name")]
        public IDictionary<string, string> Name { get; set; }

        [JsonProperty("description")]
        public IDictionary<string, string> Description { get; set; }

        /// <summary>Activity Type IRI.</summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("moreInfo")]
        public string MoreInfo { get; set; }

        /// <summary>Free-form extension bag. xAPI defines this slot explicitly.</summary>
        [JsonProperty("extensions")]
        public IDictionary<string, JToken> Extensions { get; set; }

        // ---- Interaction Activity fields (cmi5-style assessments) ----

        [JsonProperty("interactionType")]
        public string InteractionType { get; set; }

        /// <summary>
        /// Array of strings per xAPI 1.0.3 section 4.1.4.1. Tolerant binding (SDKV-16):
        /// legacy SDK emitters stringify the value (e.g. <c>"[,]"</c>);
        /// <see cref="XApiTolerantStringListConverter"/> wraps a lone JSON string
        /// into a single-element list instead of rejecting the statement.
        /// </summary>
        [JsonProperty("correctResponsesPattern")]
        [JsonConverter(typeof(XApiTolerantStringListConverter))]
        public List<string> CorrectResponsesPattern { get; set; }

        [JsonProperty("choices")]
        public List<XApiInteractionComponentDto> Choices { get; set; }

        [JsonProperty("scale")]
        public List<XApiInteractionComponentDto> Scale { get; set; }

        [JsonProperty("source")]
        public List<XApiInteractionComponentDto> Source { get; set; }

        [JsonProperty("target")]
        public List<XApiInteractionComponentDto> Target { get; set; }

        [JsonProperty("steps")]
        public List<XApiInteractionComponentDto> Steps { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>xAPI Interaction Component (spec section 4.1.4.1).</summary>
    public class XApiInteractionComponentDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public IDictionary<string, string> Description { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>xAPI Result (spec section 4.1.5).</summary>
    public class XApiResultDto
    {
        [JsonProperty("score")]
        public XApiScoreDto Score { get; set; }

        [JsonProperty("success")]
        public bool? Success { get; set; }

        [JsonProperty("completion")]
        public bool? Completion { get; set; }

        [JsonProperty("response")]
        public string Response { get; set; }

        /// <summary>ISO 8601 duration, e.g. "PT4M30S".</summary>
        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("extensions")]
        public IDictionary<string, JToken> Extensions { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>xAPI Score (spec section 4.1.5.1).</summary>
    public class XApiScoreDto
    {
        /// <summary>-1.0..1.0 inclusive.</summary>
        [JsonProperty("scaled")]
        public decimal? Scaled { get; set; }

        [JsonProperty("raw")]
        public decimal? Raw { get; set; }

        [JsonProperty("min")]
        public decimal? Min { get; set; }

        [JsonProperty("max")]
        public decimal? Max { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>
    /// xAPI Context (spec section 4.1.6).
    /// <para>
    /// <b>Febris-dialect aliases:</b> the existing JObject factor reads
    /// non-spec field names that Febris-internal producers (PC launcher,
    /// MDM, simulation plugins) emit:
    /// <list type="bullet">
    ///   <item><c>group</c> instead of spec's <c>team</c></item>
    ///   <item><c>statementreference</c> instead of spec's <c>statement</c></item>
    ///   <item><c>contextactivites</c> (sic, missing 'i') instead of spec's <c>contextActivities</c></item>
    /// </list>
    /// Both naming conventions are declared here. The typed factor
    /// reads the Febris-dialect property first, then falls back to the
    /// xAPI-spec property if dialect is null. Preserves no-behavior-
    /// change with the JObject factor while keeping the spec-shape
    /// fields available for external xAPI producers / future migration.
    /// </para>
    /// </summary>
    public class XApiContextDto
    {
        /// <summary>UUID identifying the registration session.</summary>
        [JsonProperty("registration")]
        public string Registration { get; set; }

        [JsonProperty("instructor")]
        public XApiActorDto Instructor { get; set; }

        /// <summary>xAPI spec: group of learners. Internal producers send <see cref="Group"/> instead.</summary>
        [JsonProperty("team")]
        public XApiActorDto Team { get; set; }

        /// <summary>
        /// Febris-dialect alias for <see cref="Team"/>. JObject factor reads this at SetupContext line 685.
        /// <para>
        /// Tolerant binding (SDKV-15): the wire dialect carries <c>group</c> as an
        /// ARRAY of actors (the SDK's Context.Group is List&lt;Actor&gt; and the node's
        /// JObject factor iterates it as an array), so this is typed as a list --
        /// the previous single-<see cref="XApiActorDto"/> typing made Newtonsoft throw
        /// on every real SDK statement. <see cref="XApiTolerantActorListConverter"/>
        /// additionally accepts a single actor object (spec team-style Group sent in
        /// this slot), wrapping it into a one-element list.
        /// </para>
        /// </summary>
        [JsonProperty("group")]
        [JsonConverter(typeof(XApiTolerantActorListConverter))]
        public List<XApiActorDto> Group { get; set; }

        /// <summary>xAPI spec: surrounding activities. Internal producers send <see cref="ContextActivitesTyped"/> instead.</summary>
        [JsonProperty("contextActivities")]
        public XApiContextActivitiesDto ContextActivities { get; set; }

        /// <summary>
        /// Febris-dialect alias for <see cref="ContextActivities"/>. JObject factor reads
        /// this at SetupContext line 687 -- note the TYPO ('contextactivites', missing 'i'
        /// before 'es'). Preserved verbatim so the typed factor matches existing behavior.
        /// If producers happen to be sending the correctly-spelled name, this stays null
        /// (and so does the existing JObject path -- equivalent behavior).
        /// </summary>
        [JsonProperty("contextactivites")]
        public XApiContextActivitiesDto ContextActivitesTyped { get; set; }

        [JsonProperty("revision")]
        public string Revision { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        /// <summary>RFC 5646 language tag (e.g. "en-US").</summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>xAPI spec: reference to a related statement. Internal producers send <see cref="StatementReference"/> instead.</summary>
        [JsonProperty("statement")]
        public XApiStatementRefDto Statement { get; set; }

        /// <summary>Febris-dialect alias for <see cref="Statement"/>. JObject factor reads this at SetupContext line 689.</summary>
        [JsonProperty("statementreference")]
        public XApiStatementRefDto StatementReference { get; set; }

        [JsonProperty("extensions")]
        public IDictionary<string, JToken> Extensions { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>
    /// xAPI Context Activities (spec section 4.1.6.2).
    /// <para>
    /// Tolerant binding: the Febris dialect carries each slot as ONE activity
    /// IRI string (the SDK's ContextActivities model and the node's persisted
    /// domain model are string-typed); the spec allows an Activity object or
    /// an array of them. <see cref="XApiTolerantActivityListConverter"/>
    /// accepts all three shapes (plus null), binding into these lists.
    /// </para>
    /// </summary>
    public class XApiContextActivitiesDto
    {
        [JsonProperty("parent")]
        [JsonConverter(typeof(XApiTolerantActivityListConverter))]
        public List<XApiObjectDto> Parent { get; set; }

        [JsonProperty("grouping")]
        [JsonConverter(typeof(XApiTolerantActivityListConverter))]
        public List<XApiObjectDto> Grouping { get; set; }

        [JsonProperty("category")]
        [JsonConverter(typeof(XApiTolerantActivityListConverter))]
        public List<XApiObjectDto> Category { get; set; }

        [JsonProperty("other")]
        [JsonConverter(typeof(XApiTolerantActivityListConverter))]
        public List<XApiObjectDto> Other { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>xAPI StatementRef (spec section 4.1.4.2).</summary>
    public class XApiStatementRefDto
    {
        [JsonProperty("objectType")]
        public string ObjectType { get; set; }   // "StatementRef"

        /// <summary>Statement UUID being referenced.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>
    /// xAPI Authority (spec section 4.1.8). Can be an Agent or a Group of exactly
    /// two Agents (the "Application + User" pattern documented in the spec).
    /// Modeled as XApiActorDto's superset of fields.
    /// </summary>
    public class XApiAuthorityDto
    {
        [JsonProperty("objectType")]
        public string ObjectType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mbox")]
        public string Mbox { get; set; }

        [JsonProperty("mbox_sha1sum")]
        public string MboxSha1Sum { get; set; }

        [JsonProperty("openid")]
        public string OpenId { get; set; }

        [JsonProperty("account")]
        public XApiAccountDto Account { get; set; }

        /// <summary>For Group authority, the two Agents. Tolerant binding (SDKV-14): accepts the dialect wrapper object or the spec array. See <see cref="XApiActorDto.Member"/>.</summary>
        [JsonProperty("member")]
        [JsonConverter(typeof(XApiTolerantActorListConverter))]
        public List<XApiActorDto> Member { get; set; }

        // ---- Febris-internal DB lookup hints (NOT in xAPI 1.0.3 spec) ----
        // SetupAuthority (StatementFactor line 858) requires at least one
        // of these to be present before proceeding -- it short-circuits
        // to null otherwise. Delegates the actual lookup to SetupActor,
        // which uses the same priority chain (Id -> UUID -> IFI).

        /// <summary>Febris-internal Authority.Id (numeric DB PK).</summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        /// <summary>Febris-internal Authority.UUID.</summary>
        [JsonProperty("uuid", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? UUID { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }

    /// <summary>xAPI Attachment (spec section 4.1.10).</summary>
    public class XApiAttachmentDto
    {
        /// <summary>IRI describing the purpose of the attachment (e.g. signature).</summary>
        [JsonProperty("usageType")]
        public string UsageType { get; set; }

        /// <summary>
        /// Language map per xAPI 1.0.3 section 4.1.11. Tolerant binding (SDKV-2 node
        /// side): the SDK emits attachment display/description as bare strings;
        /// <see cref="XApiTolerantLanguageMapConverter"/> wraps a lone string
        /// into <c>{"en": value}</c> instead of rejecting the statement.
        /// </summary>
        [JsonProperty("display")]
        [JsonConverter(typeof(XApiTolerantLanguageMapConverter))]
        public IDictionary<string, string> Display { get; set; }

        /// <summary>Language map; same tolerant binding as <see cref="Display"/>.</summary>
        [JsonProperty("description")]
        [JsonConverter(typeof(XApiTolerantLanguageMapConverter))]
        public IDictionary<string, string> Description { get; set; }

        /// <summary>RFC 2046 MIME content type.</summary>
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        /// <summary>Octet length.</summary>
        [JsonProperty("length")]
        public long? Length { get; set; }

        /// <summary>Hex SHA-2 of the attachment data.</summary>
        [JsonProperty("sha2")]
        public string Sha2 { get; set; }

        /// <summary>URL where the attachment content lives.</summary>
        [JsonProperty("fileUrl")]
        public string FileUrl { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }
    }
}
