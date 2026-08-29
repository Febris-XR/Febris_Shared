// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Febris.ModelLibrary.ViewModels.XApi
{
    // =====================================================================
    // Tolerant READ-side converters for the xAPI wire DTOs.
    // =====================================================================
    //
    // The node must accept BOTH wire shapes (SDK e2e verification 2026-07-17,
    // findings SDKV-14/15/16 + SDKV-2 node side; evidence in
    // the SDK end-to-end verification audit):
    //
    //   1. The Febris DIALECT -- what real SDK / PC launcher / mobile
    //      clients actually send today (lowercased keys, actor.member as
    //      a wrapper OBJECT {id, uuid, actors:[...]}, context.group as an
    //      ARRAY of actors, correctResponsesPattern / attachment display
    //      and description as bare STRINGS).
    //
    //   2. Spec-correct xAPI 1.0.3 -- the future baseline (member as an
    //      array of Agents, language maps as objects, CRP as an array of
    //      strings).
    //
    // Before these converters, default Newtonsoft binding threw
    // JsonSerializationException on shape 1 (object into List, array into
    // object, string into List/IDictionary), which XApiStatementBinding
    // caught as a ParseError -- every real SDK statement 400'd at /Submit.
    //
    // All converters are READ-ONLY (CanWrite = false): serialization keeps
    // the DTO's canonical spec-leaning shape (plain array / object), so
    // the JObject bridge in StatementLogic.Submit and any DTO round-trip
    // are unchanged on the write side.
    //
    // None of these converters throw on an unexpected token: a shape we
    // cannot interpret binds as null (the "absent" semantics every
    // downstream factor already handles) instead of rejecting the entire
    // statement at the binder.

    /// <summary>
    /// Tolerant binder for actor-list slots (<c>member</c>, <c>group</c>).
    /// Accepts:
    /// <list type="bullet">
    ///   <item>a bare JSON array of agents (xAPI 1.0.3 spec shape) -- bound as-is;</item>
    ///   <item>the Febris-dialect member wrapper object <c>{id, uuid, actors:[...]}</c>
    ///     -- its <c>actors</c> array (matched case-insensitively) is bound; the
    ///     wrapper's own id/uuid DB hints are not part of the DTO shape and are
    ///     preserved only by the raw-bytes audit capture;</item>
    ///   <item>a single actor object (spec <c>team</c>-style Group sent in the
    ///     dialect <c>group</c> slot) -- wrapped into a one-element list;</item>
    ///   <item><c>null</c> / anything else -- binds null (absent).</item>
    /// </list>
    /// </summary>
    public sealed class XApiTolerantActorListConverter : JsonConverter<List<XApiActorDto>>
    {
        /// <summary>Read-only converter: writing falls back to default list serialization.</summary>
        public override bool CanWrite => false;

        /// <summary>Never called (<see cref="CanWrite"/> is false).</summary>
        public override void WriteJson(JsonWriter writer, List<XApiActorDto> value, JsonSerializer serializer)
        {
            throw new NotSupportedException("XApiTolerantActorListConverter is read-only.");
        }

        /// <summary>Binds array / wrapper-object / single-object / null into the DTO's list shape.</summary>
        public override List<XApiActorDto> ReadJson(JsonReader reader, Type objectType, List<XApiActorDto> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Array:
                    // Spec shape: array of agents.
                    return token.ToObject<List<XApiActorDto>>(serializer);

                case JTokenType.Object:
                    JObject obj = (JObject)token;
                    // Dialect member wrapper: { id, uuid, actors: [...] }.
                    JToken actors = obj.GetValue("actors", StringComparison.OrdinalIgnoreCase);
                    if (actors != null)
                    {
                        if (actors.Type == JTokenType.Array)
                        {
                            return actors.ToObject<List<XApiActorDto>>(serializer);
                        }
                        // Wrapper present but actors is null/other: an empty membership.
                        return new List<XApiActorDto>();
                    }
                    // A single actor object (e.g. a spec-style Group sent in the
                    // dialect "group" slot): wrap into a one-element list.
                    return new List<XApiActorDto> { obj.ToObject<XApiActorDto>(serializer) };

                default:
                    // Scalars (string/number/...) carry no actor list -- absent.
                    return null;
            }
        }
    }

    /// <summary>
    /// Tolerant binder for context-activity slots (<c>contextActivities.parent
    /// / grouping / category / other</c>, xAPI 1.0.3 section 4.1.6.2). Accepts:
    /// <list type="bullet">
    ///   <item>a JSON array of Activity objects (spec shape) -- bound as-is;</item>
    ///   <item>a single Activity object (spec also allows this on the wire) --
    ///     wrapped into a one-element list;</item>
    ///   <item>a bare IRI string (the Febris dialect stores each slot as one
    ///     activity IRI string -- the SDK's ContextActivities model and the
    ///     node's persisted domain model are both string-typed) -- wrapped
    ///     into a one-element list as <c>{ id: value }</c>;</item>
    ///   <item><c>null</c> / anything else -- binds null (absent).</item>
    /// </list>
    /// Without this, a dialect statement carrying context activities threw at
    /// the binder and the whole statement 400'd -- the same reject class as
    /// SDKV-14/15 (found while regression-testing the SDKV-18 factor fix).
    /// </summary>
    public sealed class XApiTolerantActivityListConverter : JsonConverter<List<XApiObjectDto>>
    {
        /// <summary>Read-only converter: writing falls back to default list serialization.</summary>
        public override bool CanWrite => false;

        /// <summary>Never called (<see cref="CanWrite"/> is false).</summary>
        public override void WriteJson(JsonWriter writer, List<XApiObjectDto> value, JsonSerializer serializer)
        {
            throw new NotSupportedException("XApiTolerantActivityListConverter is read-only.");
        }

        /// <summary>Binds array / single-object / IRI-string / null into the DTO's list shape.</summary>
        public override List<XApiObjectDto> ReadJson(JsonReader reader, Type objectType, List<XApiObjectDto> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Array:
                    return token.ToObject<List<XApiObjectDto>>(serializer);

                case JTokenType.Object:
                    return new List<XApiObjectDto> { token.ToObject<XApiObjectDto>(serializer) };

                case JTokenType.String:
                    // Dialect shape: one activity IRI string per slot.
                    return new List<XApiObjectDto> { new XApiObjectDto { Id = (string)token } };

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Tolerant binder for <c>correctResponsesPattern</c> (xAPI 1.0.3
    /// section 4.1.4.1: array of strings). A JSON array passes through; a lone
    /// JSON string (legacy SDK emitters stringify the value, SDKV-1/16)
    /// wraps into a single-element list; null / other shapes bind null.
    /// </summary>
    public sealed class XApiTolerantStringListConverter : JsonConverter<List<string>>
    {
        /// <summary>Read-only converter: writing falls back to default list serialization.</summary>
        public override bool CanWrite => false;

        /// <summary>Never called (<see cref="CanWrite"/> is false).</summary>
        public override void WriteJson(JsonWriter writer, List<string> value, JsonSerializer serializer)
        {
            throw new NotSupportedException("XApiTolerantStringListConverter is read-only.");
        }

        /// <summary>Binds array / lone-string / null into the DTO's list-of-strings shape.</summary>
        public override List<string> ReadJson(JsonReader reader, Type objectType, List<string> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Array:
                    return token.ToObject<List<string>>(serializer);

                case JTokenType.String:
                    // Legacy emitters collapse the pattern to one string.
                    return new List<string> { (string)token };

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Tolerant binder for language-map slots (attachment <c>display</c> /
    /// <c>description</c>, xAPI 1.0.3 section 4.1.11). A JSON object passes
    /// through as the dictionary; a lone JSON string (the SDK emits plain
    /// strings for attachments, SDKV-2) wraps into <c>{"en": value}</c>;
    /// null / other shapes bind null. Non-string map values are stringified
    /// rather than thrown on, so a malformed producer can never 400 the
    /// whole statement here.
    /// </summary>
    public sealed class XApiTolerantLanguageMapConverter : JsonConverter<IDictionary<string, string>>
    {
        /// <summary>Language tag used when wrapping a bare string into a map.</summary>
        public const string DefaultLanguageTag = "en";

        /// <summary>Read-only converter: writing falls back to default dictionary serialization.</summary>
        public override bool CanWrite => false;

        /// <summary>Never called (<see cref="CanWrite"/> is false).</summary>
        public override void WriteJson(JsonWriter writer, IDictionary<string, string> value, JsonSerializer serializer)
        {
            throw new NotSupportedException("XApiTolerantLanguageMapConverter is read-only.");
        }

        /// <summary>Binds object / lone-string / null into the DTO's language-map shape.</summary>
        public override IDictionary<string, string> ReadJson(JsonReader reader, Type objectType, IDictionary<string, string> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            JToken token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Object:
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    foreach (JProperty property in ((JObject)token).Properties())
                    {
                        if (property.Value.Type == JTokenType.Null)
                        {
                            map[property.Name] = null;
                        }
                        else if (property.Value.Type == JTokenType.String)
                        {
                            map[property.Name] = (string)property.Value;
                        }
                        else
                        {
                            // Never throw on a weird value shape -- stringify.
                            map[property.Name] = property.Value.ToString(Formatting.None);
                        }
                    }
                    return map;

                case JTokenType.String:
                    // Dialect shape: bare string -> single-entry map.
                    return new Dictionary<string, string> { { DefaultLanguageTag, (string)token } };

                default:
                    return null;
            }
        }
    }
}
