// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Febris.EnumLibrary
{
    public enum ExtensionIRIOptions
    {
        RestartCounterIRI,
        NotesIRI,
        // FIX (LMS-B3): safe sentinel returned by GetVerbEnum for an unrecognized extension IRI
        // instead of throwing. The result-extras extractor switches on this value with no default
        // branch, so an Unknown entry is skipped while RestartCount and Notes from the recognized
        // entries are still extracted.
        Unknown,
    }

    public class ExtensionIRIResolver
    {
        public static string ResolveExtensionIRI(ExtensionIRIOptions iri)
        {
            switch (iri)
            {
                case ExtensionIRIOptions.RestartCounterIRI:
                    return "http://febr.is/extensions/resultextensions/restartcounter";
                case ExtensionIRIOptions.NotesIRI:
                    return "http://febr.is/extensions/resultextensions/notes";
                // FIX (LMS-B3): the Unknown sentinel has no concrete IRI. Return an empty string
                // rather than letting it fall through to the throwing default, so introducing the
                // sentinel does not create a new throw path.
                case ExtensionIRIOptions.Unknown:
                    return string.Empty;
                default:
                    // Handle bad URL, possibly throw
                    throw new Exception();
            }
        }
        public static ExtensionIRIOptions GetVerbEnum(string currentExtension)
        {
            switch (currentExtension)
            {
                case "http://febr.is/extensions/resultextensions/restartcounter":
                    return ExtensionIRIOptions.RestartCounterIRI;
                case "http://febr.is/extensions/resultextensions/notes":
                    return ExtensionIRIOptions.NotesIRI;
                // FIX (LMS-B3): an unrecognized extension IRI now returns the Unknown sentinel
                // instead of throwing. The old throw bubbled through the caller catch in
                // StatementFactory.FactorResultExtensionExtras, which aborted the extension-map
                // loop and returned null, silently dropping RestartCount and Notes for the whole
                // result. The caller switches on the returned value with no default branch, so an
                // Unknown entry is skipped and the recognized entries are still extracted. Wiring
                // the caller to the bounds-checked TryParseExtensionEntry helper below and
                // reconciling the divergent extras copies is the remaining structural work.
                default:
                    // Handle bad URL, possibly throw
                    // FIX (LMS-B3): old throwing behavior preserved here as a comment per the
                    // comment-out-do-not-delete rule.
                    // throw new Exception();
                    return ExtensionIRIOptions.Unknown;
            }
        }
    }

    /// <summary>
    /// FIX (LMS-B3): pure, bounds-checked parse of a single xAPI result extension-map entry,
    /// lifted out so the corrected split-and-resolve logic can be unit-tested without standing
    /// up StatementFactory (whose FactorResultExtensionExtras news a Result and swallows every
    /// exception to null, leaving no test seam until the extras reconciliation lands). It mirrors
    /// the inline parse at StatementFactory.FactorResultExtensionExtras (the "key = parts[0] + ':'
    /// + parts[1]" then GetVerbEnum(key) then "value = parts[2]" sequence) but guards the colon
    /// split before indexing, so a malformed entry with fewer than three colon-separated parts
    /// returns false instead of throwing IndexOutOfRangeException and aborting the whole loop.
    /// An unrecognized but well-formed IRI resolves to the Unknown sentinel and still returns
    /// true with that sentinel, leaving the skip decision to the caller switch.
    /// </summary>
    public static class ExtensionMapParsing
    {
        // The recognized keys are "scheme://host/path:fragment" style, so the key is the first two
        // colon-separated parts joined back with a colon and the value is the third part. Anything
        // shorter than three parts cannot carry a value and is treated as malformed.
        private const int MinimumParts = 3;

        // FIX (LMS-B3): bounds-checked colon split. Returns false (and Unknown / null) for a null,
        // empty, or short entry rather than indexing past the end of the split array. On a
        // well-formed entry it resolves the key through the non-throwing GetVerbEnum, so an
        // unrecognized IRI yields iri = ExtensionIRIOptions.Unknown with success = true and the
        // caller's default-less switch simply skips it.
        public static bool TryParseExtensionEntry(string rawEntry, out ExtensionIRIOptions iri, out string value)
        {
            iri = ExtensionIRIOptions.Unknown;
            value = null;
            if (string.IsNullOrEmpty(rawEntry))
            {
                return false;
            }
            string[] parts = rawEntry.Split(':');
            if (parts.Length < MinimumParts)
            {
                return false;
            }
            string key = parts[0] + ":" + parts[1];
            iri = ExtensionIRIResolver.GetVerbEnum(key);
            value = parts[2];
            return true;
        }
    }
}
