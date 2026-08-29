// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using Febris.SharedServices;
using FluentAssertions;
using Xunit;

namespace Febris.SharedServices.Tests
{
    /// <summary>
    /// Pins <see cref="MediaPathGuard"/> -- the root-containment guard for audit C-08
    /// (authenticated arbitrary file read via the Portal's media loaders).
    ///
    /// <para>
    /// The defect was that <c>Path.Combine(root, userValue)</c> DISCARDS the root entirely when
    /// <c>userValue</c> is rooted, so <c>?path=D:\...\appsettings.Development.json</c> read that
    /// file instead of an image -- substitution, not traversal. Plain <c>..\..\</c> also worked,
    /// because Combine never normalises. The gate above these loaders admits the lowest roles, so
    /// "any authenticated user" could read the node's live database credentials.
    /// </para>
    ///
    /// <para>
    /// The must-not-regress cases are the ACCEPT ones. Stripping to <c>Path.GetFileName</c> would
    /// have been simpler and would have blocked every attack here -- and silently broken logos
    /// (<c>Logos\{uuid}{ext}</c>) and publication images
    /// (<c>{uuid}\Images\{guid}{ext}</c>), which are legitimately multi-segment.
    /// </para>
    /// </summary>
    public class MediaPathGuardTests
    {
        private static readonly string Root = Path.Combine(Path.GetTempPath(), "febris-media-root");

        // --- Rejections: the actual C-08 attack shapes ---

        [Fact]
        public void RootedPath_IsRefused_ThisIsTheReportedDefect()
        {
            // The exact shape from the audit: a rooted second argument that Path.Combine would
            // have honoured wholesale, discarding the media root.
            //
            // The Windows arm used to be this developer's real checkout path. It ships: this file
            // is inside the public export, so it published the maintainer's local directory layout
            // and the internal codename together, which is precisely what the NR-19 sweep exists to
            // catch. Replaced 2026-08-25 with a synthetic path that exercises the identical
            // behaviour -- what makes this input dangerous is that it is ROOTED, not where it
            // happens to point.
            string rooted = Path.DirectorySeparatorChar == '\\'
                ? @"C:\Windows\System32\config\SAM"
                : "/etc/passwd";

            MediaPathGuard.TryResolve(Root, rooted, out string full).Should().BeFalse();
            full.Should().BeNull();
        }

        [Theory]
        [InlineData("../../../etc/passwd")]
        [InlineData(@"..\..\..\Windows\win.ini")]
        [InlineData("subdir/../../escape.txt")]
        [InlineData(@"Logos\..\..\appsettings.Development.json")]
        public void Traversal_IsRefused(string attempt)
        {
            MediaPathGuard.TryResolve(Root, attempt, out string full).Should().BeFalse();
            full.Should().BeNull();
        }

        [Theory]
        [InlineData(@"..\..\..\Windows\win.ini", "../../../Windows/win.ini")]
        [InlineData(@"Logos\..\..\appsettings.Development.json", "Logos/../../appsettings.Development.json")]
        [InlineData(@"Logos\..\Badges\b.png", "Logos/../Badges/b.png")]
        public void BackslashAndForwardSlashFormsAgree(string backslashForm, string forwardSlashForm)
        {
            // REGRESSION PIN, 2026-08-27. Backslash is a path separator on Windows and an ORDINARY
            // FILENAME CHARACTER on Unix. Before the Replace in MediaPathGuard, the first two rows
            // were REFUSED on Windows and ACCEPTED on Linux, because nothing collapsed the `..`
            // segments there -- the value stayed one long filename sitting inside the root. So this
            // suite was green on the dev box and would have been RED in the container the node
            // actually ships in, which is exactly the class of defect CI exists to catch and could
            // not, because CI had never run.
            //
            // This asserts the two spellings AGREE rather than asserting either verdict, so it keeps
            // its meaning on whichever OS it runs on. The third row is an ACCEPT case, so a guard
            // that simply refused everything containing a backslash would not satisfy it either.
            bool backslashAccepted = MediaPathGuard.TryResolve(Root, backslashForm, out string fromBackslash);
            bool forwardAccepted = MediaPathGuard.TryResolve(Root, forwardSlashForm, out string fromForward);

            backslashAccepted.Should().Be(forwardAccepted);
            fromBackslash.Should().Be(fromForward);
        }

        [Fact]
        public void UncAndVolumeQualifiedPaths_AreRefused_OnEveryPlatform()
        {
            // The node ships in a Linux container but is developed on Windows. A value that is
            // "rooted" only on one of those must be refused on both, or the same request behaves
            // differently per environment.
            MediaPathGuard.TryResolve(Root, @"\\evil-host\share\secret.txt", out _).Should().BeFalse();
            MediaPathGuard.TryResolve(Root, @"C:\Windows\win.ini", out _).Should().BeFalse();
            MediaPathGuard.TryResolve(Root, "C:secret.txt", out _).Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void EmptyInput_IsRefused(string input)
        {
            MediaPathGuard.TryResolve(Root, input, out string full).Should().BeFalse();
            full.Should().BeNull();
        }

        [Fact]
        public void SiblingDirectoryWithSharedPrefix_IsRefused()
        {
            // "/rootevil" must not pass as being inside "/root". This is why the comparison appends
            // a trailing separator before the StartsWith.
            string sibling = Path.Combine(Path.GetTempPath(), "febris-media-root-evil");
            Directory.CreateDirectory(Path.GetDirectoryName(sibling) ?? Path.GetTempPath());

            MediaPathGuard.TryResolve(Root, "../febris-media-root-evil/secret.txt", out string full)
                .Should().BeFalse();
            full.Should().BeNull();
        }

        [Fact]
        public void MissingRoot_IsRefused()
        {
            MediaPathGuard.TryResolve(null, "x.png", out _).Should().BeFalse();
            MediaPathGuard.TryResolve("", "x.png", out _).Should().BeFalse();
        }

        // --- Acceptances: the values the loaders legitimately receive ---

        [Fact]
        public void BareFilename_IsAccepted()
        {
            MediaPathGuard.TryResolve(Root, "8f14e45f.png", out string full).Should().BeTrue();
            full.Should().StartWith(Path.GetFullPath(Root));
            full.Should().EndWith("8f14e45f.png");
        }

        [Theory]
        [InlineData(@"Logos\8f14e45f-0000-0000-0000-000000000000.png")]
        [InlineData("Logos/8f14e45f-0000-0000-0000-000000000000.png")]
        public void MultiSegmentLogoPath_IsAccepted_BecauseThatIsHowLogosAreStored(string stored)
        {
            // AccreditationBodyLogic / InstitutionLogic / ContentDeveloperLogic all write
            // LogoFileSystemPathForDb ("Logos\") + uuid + ext. Path.GetFileName would refuse these.
            MediaPathGuard.TryResolve(Root, stored, out string full).Should().BeTrue();
            full.Should().StartWith(Path.GetFullPath(Root));
        }

        [Fact]
        public void DeeplyNestedPublicationImage_IsAccepted()
        {
            // Publication images are written as {publicationUUID}\Images\{guid}{ext}.
            MediaPathGuard.TryResolve(
                Root,
                @"3f2aed6c-0000-0000-0000-000000000000\Images\1a2b3c4d.jpg",
                out string full).Should().BeTrue();
            full.Should().StartWith(Path.GetFullPath(Root));
        }

        [Fact]
        public void LeadingSeparator_IsRefused_BecauseWindowsTreatsItAsRooted()
        {
            // Path.IsPathRooted("/Logos/a.png") is TRUE on Windows -- a leading separator means
            // "root of the current drive" -- and true on Unix for the obvious reason. So the guard
            // refuses it before the relative-path handling is reached.
            //
            // That is deliberate and costs nothing: no writer produces a leading separator. The
            // stored conventions are "Logos\{uuid}{ext}" and "{uuid}\Images\{guid}{ext}", both
            // separator-free at the front. Refusing is the fail-closed choice, and the alternative
            // (trim it, then resolve) would mean the same request behaved differently on Windows
            // and in the Linux container.
            //
            // Recorded because the first version of this test asserted the OPPOSITE and failed --
            // the expectation was wrong, not the guard.
            MediaPathGuard.TryResolve(Root, "/Logos/a.png", out string full).Should().BeFalse();
            full.Should().BeNull();
        }

        [Fact]
        public void InnerTraversalThatStaysInsideTheRoot_IsAccepted()
        {
            // Containment, not paranoia about the ".." token itself: this normalises back inside.
            MediaPathGuard.TryResolve(Root, @"Logos\..\Badges\b.png", out string full).Should().BeTrue();
            full.Should().StartWith(Path.GetFullPath(Root));
            full.Should().EndWith("b.png");
        }
    }
}
