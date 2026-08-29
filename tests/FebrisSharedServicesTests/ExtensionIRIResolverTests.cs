// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins the LMS-B3 fix in shared/FebrisEnumLibrary/ExtensionIRIOptions.cs: GetVerbEnum returns
    /// the Unknown sentinel instead of throwing on an unrecognized extension IRI, the enum-to-string
    /// resolver tolerates that sentinel, and the lifted ExtensionMapParsing.TryParseExtensionEntry
    /// bounds-checks the colon split so a malformed entry cannot throw IndexOutOfRangeException and
    /// abort the result-extras loop.
    /// </summary>
    public class ExtensionIRIResolverTests
    {
        private const string RestartCounterIri = "http://febr.is/extensions/resultextensions/restartcounter";
        private const string NotesIri = "http://febr.is/extensions/resultextensions/notes";

        // ---- GetVerbEnum: recognized IRIs still map to their options ----

        [Fact]
        public void GetVerbEnum_restartCounterIri_returnsRestartCounterOption()
        {
            Assert.Equal(ExtensionIRIOptions.RestartCounterIRI, ExtensionIRIResolver.GetVerbEnum(RestartCounterIri));
        }

        [Fact]
        public void GetVerbEnum_notesIri_returnsNotesOption()
        {
            Assert.Equal(ExtensionIRIOptions.NotesIRI, ExtensionIRIResolver.GetVerbEnum(NotesIri));
        }

        // ---- GetVerbEnum: the LMS-B3 regression guard ----

        [Fact]
        public void GetVerbEnum_unrecognizedIri_returnsUnknownAndDoesNotThrow()
        {
            // The bug: the default case threw a bare Exception, which bubbled through the caller
            // catch in StatementFactory and dropped RestartCount and Notes for the whole result.
            // The fix returns the Unknown sentinel, which the caller's default-less switch skips.
            Assert.Equal(ExtensionIRIOptions.Unknown, ExtensionIRIResolver.GetVerbEnum("http://example.org/extensions/custom"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-an-iri")]
        [InlineData("http://febr.is/extensions/resultextensions/restartcounter:trailing")]
        public void GetVerbEnum_unmatchedInputs_returnUnknown(string input)
        {
            Assert.Equal(ExtensionIRIOptions.Unknown, ExtensionIRIResolver.GetVerbEnum(input));
        }

        // ---- ResolveExtensionIRI: enum-to-string round trip and the Unknown sentinel ----

        [Fact]
        public void ResolveExtensionIRI_restartCounterOption_returnsRestartCounterIri()
        {
            Assert.Equal(RestartCounterIri, ExtensionIRIResolver.ResolveExtensionIRI(ExtensionIRIOptions.RestartCounterIRI));
        }

        [Fact]
        public void ResolveExtensionIRI_notesOption_returnsNotesIri()
        {
            Assert.Equal(NotesIri, ExtensionIRIResolver.ResolveExtensionIRI(ExtensionIRIOptions.NotesIRI));
        }

        [Fact]
        public void ResolveExtensionIRI_unknownSentinel_returnsEmptyAndDoesNotThrow()
        {
            // Adding the Unknown member must not create a new throw path through the enum-to-string
            // overload, so the sentinel resolves to an empty string rather than the throwing default.
            Assert.Equal(string.Empty, ExtensionIRIResolver.ResolveExtensionIRI(ExtensionIRIOptions.Unknown));
        }

        [Fact]
        public void GetVerbEnum_roundTripsThroughResolveForKnownOptions()
        {
            Assert.Equal(ExtensionIRIOptions.RestartCounterIRI,
                ExtensionIRIResolver.GetVerbEnum(ExtensionIRIResolver.ResolveExtensionIRI(ExtensionIRIOptions.RestartCounterIRI)));
            Assert.Equal(ExtensionIRIOptions.NotesIRI,
                ExtensionIRIResolver.GetVerbEnum(ExtensionIRIResolver.ResolveExtensionIRI(ExtensionIRIOptions.NotesIRI)));
        }

        // ---- ExtensionMapParsing.TryParseExtensionEntry: bounds-checked colon split ----

        [Fact]
        public void TryParseExtensionEntry_restartCounterEntry_parsesIriAndValue()
        {
            // A full extension-map entry is "<iri>:<value>". Split on ':' the recognized restart
            // counter IRI plus a value yields three parts: scheme, host-and-path, value.
            bool ok = ExtensionMapParsing.TryParseExtensionEntry(RestartCounterIri + ":3", out ExtensionIRIOptions iri, out string value);
            Assert.True(ok);
            Assert.Equal(ExtensionIRIOptions.RestartCounterIRI, iri);
            Assert.Equal("3", value);
        }

        [Fact]
        public void TryParseExtensionEntry_notesEntry_parsesIriAndPipeDelimitedValue()
        {
            bool ok = ExtensionMapParsing.TryParseExtensionEntry(NotesIri + ":first|second", out ExtensionIRIOptions iri, out string value);
            Assert.True(ok);
            Assert.Equal(ExtensionIRIOptions.NotesIRI, iri);
            Assert.Equal("first|second", value);
        }

        [Fact]
        public void TryParseExtensionEntry_unrecognizedButWellFormedIri_succeedsWithUnknown()
        {
            // A well-formed but unrecognized IRI is not a parse failure. It resolves to the Unknown
            // sentinel and returns true, leaving the skip decision to the caller switch.
            bool ok = ExtensionMapParsing.TryParseExtensionEntry("http://example.org/x:42", out ExtensionIRIOptions iri, out string value);
            Assert.True(ok);
            Assert.Equal(ExtensionIRIOptions.Unknown, iri);
            Assert.Equal("42", value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("onlyonepart")]
        [InlineData("http://febr.is/extensions/resultextensions/restartcounter")]
        public void TryParseExtensionEntry_shortOrEmptyEntry_returnsFalseAndDoesNotThrow(string rawEntry)
        {
            // The bug: the caller indexed parts[0], parts[1], parts[2] with no length check, so an
            // entry with fewer than three colon-separated parts threw IndexOutOfRangeException and
            // aborted the whole extension-map loop. The fix returns false instead.
            bool ok = ExtensionMapParsing.TryParseExtensionEntry(rawEntry, out ExtensionIRIOptions iri, out string value);
            Assert.False(ok);
            Assert.Equal(ExtensionIRIOptions.Unknown, iri);
            Assert.Null(value);
        }

        [Fact]
        public void TryParseExtensionEntry_extraColonsInValue_keepsOnlyThirdPartAsValue()
        {
            // Behavior-preserving: the original parse took parts[2] as the value, so a value that
            // itself contains a colon is split and only the segment up to the next colon is kept.
            // This documents the lifted logic rather than asserting a new contract.
            bool ok = ExtensionMapParsing.TryParseExtensionEntry(NotesIri + ":a:b", out ExtensionIRIOptions iri, out string value);
            Assert.True(ok);
            Assert.Equal(ExtensionIRIOptions.NotesIRI, iri);
            Assert.Equal("a", value);
        }
    }
}
