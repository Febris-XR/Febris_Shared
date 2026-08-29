// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Source-level guard on <c>JwtSigningKeyProvider</c>.
    ///
    /// <para>
    /// MOVED HERE 2026-08-29 from the node's ConfigurationSurfaceGuardTests. It reads a file that
    /// ships in THIS repository, so it belongs with it. The rest of that suite stayed in the node:
    /// every other test there targets enduser/ config, and its references to shared/ are scan ROOTS
    /// for the node's dependency graph, not assertions about shared code. Moving the whole file
    /// would have put node config assertions in a repository with no enduser/ at all.
    /// </para>
    /// </summary>
    public class JwtSigningKeyProviderSourceGuardTests
    {
        /// <summary>
        /// Repo root, identified by carrying <c>shared/</c>. ONE directory on purpose: the node's
        /// equivalents were bitten three times by markers a cut legitimately removes, and shared/ is
        /// the one tree both this repository and the workshop always have.
        /// </summary>
        private static string FindRepoRoot()
        {
            DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "shared")))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
            throw new InvalidOperationException("Could not locate the repo root (a directory containing shared/).");
        }

        /// <summary>
        /// Blank out comments line-wise. The node's guard uses a character-scanner that also handles
        /// verbatim literals, because it scans EVERY source file. This one reads a single known file,
        /// and the test below pins the precondition that makes the simpler form sound. Duplicating
        /// eighty lines of scanner to cover a case this file does not contain would create exactly
        /// the twin drift the duplicate-type guard exists to catch.
        /// </summary>
        private static string StripComments(string source)
        {
            StringBuilder outp = new StringBuilder(source.Length);
            bool inBlock = false;
            foreach (string raw in source.Split('\n'))
            {
                string line = raw;
                if (inBlock)
                {
                    int end = line.IndexOf("*/", StringComparison.Ordinal);
                    if (end < 0) { outp.Append('\n'); continue; }
                    line = line.Substring(end + 2);
                    inBlock = false;
                }
                int open = line.IndexOf("/*", StringComparison.Ordinal);
                if (open >= 0)
                {
                    inBlock = true;
                    line = line.Substring(0, open);
                }
                int slash = line.IndexOf("//", StringComparison.Ordinal);
                if (slash >= 0)
                {
                    line = line.Substring(0, slash);
                }
                outp.Append(line).Append('\n');
            }
            return outp.ToString();
        }

        private static string ProviderPath()
        {
            return Path.Combine(FindRepoRoot(), "shared", "FebrisSharedServices", "JwtSigningKeyProvider.cs");
        }

        [Fact]
        public void The_simple_comment_strip_is_still_sound_for_this_file()
        {
            // The line-wise strip above is only correct while the file contains no verbatim string
            // and no comment marker inside a literal. Both were true when this moved. If either
            // changes, this fails FIRST and tells you to bring the character-scanner across rather
            // than letting the guard below quietly go blind.
            string raw = File.ReadAllText(ProviderPath());
            Assert.DoesNotContain("@\"", raw);

            // PER LINE, deliberately. A whole-file regex here matched across a newline, from
            // the quote in an XML doc comment's type="bullet" to the /// beginning the next
            // line, and reported a literal that does not exist. The property being pinned is
            // per-line anyway: a comment marker sitting inside a string ON THE SAME LINE.
            Regex marker = new Regex(@"""[^""\r\n]*(//|/\*)");
            string[] lines = raw.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                Assert.False(marker.IsMatch(lines[i]),
                    "line " + (i + 1) + " puts a comment marker inside a string literal, so the "
                    + "line-wise strip below is no longer sound: " + lines[i].Trim());
            }
        }

        [Fact]
        public void The_provider_evaluates_production_validation_in_every_environment()
        {
            // The mechanism behind the development-secret waiver: validation runs everywhere and
            // Development only decides what to DO with a failure. The old shape --
            // `if (isDevelopment) return;` ahead of the checks -- cannot report anything, because it
            // never looks. Pinned at source level because no unit test can tell "returned early"
            // from "checked and waived" when both end in a constructed provider.
            string live = StripComments(File.ReadAllText(ProviderPath()));

            // Matches `return;` and `return null;` gated directly on isDevelopment -- both are the
            // silent shape. The legitimate carve-out returns the computed REASON, which this pattern
            // deliberately does not match.
            Assert.False(
                Regex.IsMatch(live, @"if\s*\(\s*isDevelopment\s*\)\s*\{?\s*return\s*(null)?\s*;"),
                "ValidateOrThrow must not early-return on isDevelopment before the checks -- that is the silent carve-out ROADMAP 18 removed");
            Assert.Contains("ProductionRejectionReason(", live);
        }
    }
}
