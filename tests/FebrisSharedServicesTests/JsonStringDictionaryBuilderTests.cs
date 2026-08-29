// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using Febris.SharedServices;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Tests for <see cref="JsonStringDictionaryBuilder"/>.
    ///
    /// <para>
    /// The builder serializes and deserializes <c>Dictionary&lt;string,string&gt;</c> instances to and from
    /// JSON strings. It is used in places where the platform stores language-keyed text (xAPI
    /// LanguageMap, for example). Every public method wraps its body in try / throw, so the
    /// tests assert both the happy path and the exception-throwing path.
    /// </para>
    /// </summary>
    public class JsonStringDictionaryBuilderTests
    {
        private readonly JsonStringDictionaryBuilder _sut = new JsonStringDictionaryBuilder();

        // --- NewJsonDictionaryStringBuilder ---------------------------------------------------

        [Fact]
        public void NewJsonDictionaryStringBuilder_ReturnsJsonObjectWithLanguageKey()
        {
            var result = _sut.NewJsonDictionaryStringBuilder("en-US", "Hello");

            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            parsed.Should().ContainKey("en-US").WhoseValue.Should().Be("Hello");
            parsed.Should().HaveCount(1);
        }

        [Fact]
        public void NewJsonDictionaryStringBuilder_WithUnicodeValue_PreservesCharacters()
        {
            var result = _sut.NewJsonDictionaryStringBuilder("ja-JP", "こんにちは");

            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            parsed["ja-JP"].Should().Be("こんにちは");
        }

        // --- UpdateJsonDictionaryStringBuilder ------------------------------------------------

        [Fact]
        public void UpdateJsonDictionaryStringBuilder_AddsNewLanguageToExistingJson()
        {
            var startingJson = _sut.NewJsonDictionaryStringBuilder("en-US", "Hello");

            var result = _sut.UpdateJsonDictionaryStringBuilder(startingJson, "fr-FR", "Bonjour");

            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            parsed.Should().HaveCount(2);
            parsed["en-US"].Should().Be("Hello");
            parsed["fr-FR"].Should().Be("Bonjour");
        }

        [Fact]
        public void UpdateJsonDictionaryStringBuilder_OnDuplicateKey_Throws()
        {
            // Dictionary.Add throws ArgumentException on duplicate key, which is rethrown by the helper.
            var startingJson = _sut.NewJsonDictionaryStringBuilder("en-US", "Hello");

            Action act = () => _sut.UpdateJsonDictionaryStringBuilder(startingJson, "en-US", "Hi");

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UpdateJsonDictionaryStringBuilder_WithMalformedJson_Throws()
        {
            // Malformed input -> JsonSerializationException / JsonReaderException -> rethrown.
            Action act = () => _sut.UpdateJsonDictionaryStringBuilder("{not json}", "en-US", "Hello");

            act.Should().Throw<JsonException>();
        }

        // --- ConvertStringToJsonStringArrayString ---------------------------------------------

        [Fact]
        public void ConvertStringToJsonStringArrayString_BasicTwoKeyInput_ProducesJsonDictionary()
        {
            // Format: "key1:value1,key2:value2".
            var result = _sut.ConvertStringToJsonStringArrayString("step1:open the door,step2:close the door");

            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            parsed.Should().HaveCount(2);
            parsed["step1"].Should().Be("open the door");
            parsed["step2"].Should().Be("close the door");
        }

        [Fact]
        public void ConvertStringToJsonStringArrayString_StripsCarriageReturnsAndLinefeeds()
        {
            // The implementation replaces "\r\n" with empty string before splitting on ','.
            var result = _sut.ConvertStringToJsonStringArrayString("a:one,\r\nb:two");

            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            parsed.Should().ContainKey("a").WhoseValue.Should().Be("one");
            parsed.Should().ContainKey("b").WhoseValue.Should().Be("two");
        }

        [Fact]
        public void ConvertStringToJsonStringArrayString_WithEmptyEntriesBetweenCommas_SkipsThem()
        {
            // Split uses StringSplitOptions.RemoveEmptyEntries -> ",,,a:one,,," reduces to one entry.
            var result = _sut.ConvertStringToJsonStringArrayString(",,,a:one,,,");

            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            parsed.Should().ContainKey("a").WhoseValue.Should().Be("one");
            parsed.Should().HaveCount(1);
        }

        [Fact]
        public void ConvertStringToJsonStringArrayString_WithEntryMissingColon_Throws()
        {
            // "a" has no ':' delimiter -> i[1] index-out-of-range during dictionary build -> rethrown.
            Action act = () => _sut.ConvertStringToJsonStringArrayString("a,b:two");

            act.Should().Throw<IndexOutOfRangeException>();
        }

        [Fact]
        public void ConvertStringToJsonStringArrayString_WithNullInput_Throws()
        {
            // The helper invokes Replace on the input first; null.Replace -> NullReferenceException.
            Action act = () => _sut.ConvertStringToJsonStringArrayString(null);

            act.Should().Throw<NullReferenceException>();
        }

        // --- GetLanguageFromJsonDictionary ----------------------------------------------------

        [Fact]
        public void GetLanguageFromJsonDictionary_ReturnsFirstKey()
        {
            var json = _sut.NewJsonDictionaryStringBuilder("en-US", "Hello");

            var result = _sut.GetLanguageFromJsonDictionary(json);

            result.Should().Be("en-US");
        }

        [Fact]
        public void GetLanguageFromJsonDictionary_WithEmptyDictionary_Throws()
        {
            // .Keys.First() on an empty sequence throws InvalidOperationException.
            Action act = () => _sut.GetLanguageFromJsonDictionary("{}");

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetLanguageFromJsonDictionary_WithMalformedJson_Throws()
        {
            Action act = () => _sut.GetLanguageFromJsonDictionary("{not json}");

            act.Should().Throw<JsonException>();
        }

        // --- GetValueFromDictionaryWithKey ----------------------------------------------------

        [Fact]
        public void GetValueFromDictionaryWithKey_ReturnsValueForExistingKey()
        {
            var json = _sut.NewJsonDictionaryStringBuilder("en-US", "Hello");

            var result = _sut.GetValueFromDictionaryWithKey(json, "en-US");

            result.Should().Be("Hello");
        }

        [Fact]
        public void GetValueFromDictionaryWithKey_ReturnsNullForMissingKey()
        {
            // TryGetValue out is default(string) (null) when key absent; the helper returns it as-is.
            var json = _sut.NewJsonDictionaryStringBuilder("en-US", "Hello");

            var result = _sut.GetValueFromDictionaryWithKey(json, "fr-FR");

            result.Should().BeNull();
        }
    }
}
