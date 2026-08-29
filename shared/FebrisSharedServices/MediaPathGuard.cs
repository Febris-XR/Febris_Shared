// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;

namespace Febris.SharedServices
{
    /// <summary>
    /// Root-containment guard for the legacy media loaders (audit C-08).
    ///
    /// <para>
    /// Those loaders did <c>Path.Combine(root, userSuppliedValue)</c> and handed the result straight
    /// to <c>File.ReadAllBytes</c> / <c>new FileStream</c>. <see cref="Path.Combine(string,string)"/>
    /// <b>discards the first argument entirely</b> when the second is rooted, so
    /// <c>?path=D:\...\appsettings.Development.json</c> was not traversal at all -- it was
    /// substitution, and it read whatever the process could read. Plain <c>..\..\</c> worked too,
    /// because Combine never normalises.
    /// </para>
    ///
    /// <para>
    /// The check is deliberately CONTAINMENT rather than <c>Path.GetFileName</c>. Stripping to a
    /// bare filename would be simpler and is safe for some of these loaders, but it silently breaks
    /// the ones whose stored values are legitimately multi-segment -- logos are written as
    /// <c>Logos\{uuid}{ext}</c>, publication images as <c>{publicationUUID}\Images\{guid}{ext}</c>.
    /// Containment accepts every legitimate value, bare or nested, and rejects anything that lands
    /// outside the root.
    /// </para>
    ///
    /// <para>
    /// Logic mirrors <c>FileSystemStorageProvider.Resolve</c>, which the audit reviewed as a correct
    /// traversal guard. It is duplicated here rather than reused because that method is private to
    /// the storage provider and bound to its base path and forward-slash key semantics, and because
    /// <c>StorageKeys</c> deliberately does not yet cover the media areas -- routing these loaders
    /// through <c>IStorageProvider</c> is blocked on the Phase 3 layout reconciliation. When that
    /// lands, these call sites should move to the storage seam and this class should be deleted.
    /// Note also that the S3 provider's key normalisation has no traversal check, so "route it
    /// through IStorageProvider" would not by itself have been a fix.
    /// </para>
    /// </summary>
    public static class MediaPathGuard
    {
        /// <summary>
        /// Resolve <paramref name="userSuppliedPath"/> beneath <paramref name="root"/>.
        /// Returns false -- and sets <paramref name="fullPath"/> to null -- when the value is empty,
        /// is rooted (absolute), or resolves outside the root. Never throws for bad input: these are
        /// request values, so a refusal is an expected outcome, not an exceptional one.
        /// </summary>
        public static bool TryResolve(string root, string userSuppliedPath, out string fullPath)
        {
            fullPath = null;

            if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(userSuppliedPath))
            {
                return false;
            }

            // Reject a rooted value outright rather than letting Path.Combine silently discard the
            // base. This is the actual C-08 defect and it deserves its own explicit refusal.
            if (Path.IsPathRooted(userSuppliedPath))
            {
                return false;
            }

            // A value containing a volume/UNC prefix is rooted on Windows but may not be flagged by
            // IsPathRooted on Unix, so refuse it explicitly too -- the node ships in a Linux
            // container and the same request must be refused on both.
            if (userSuppliedPath.Contains(":") || userSuppliedPath.StartsWith("\\\\", StringComparison.Ordinal))
            {
                return false;
            }

            string candidate;
            string resolvedRoot;
            try
            {
                resolvedRoot = Path.GetFullPath(root);
                // Normalise backslash to forward slash BEFORE trimming and combining. Backslash is a
                // separator on Windows and an ORDINARY FILENAME CHARACTER on Unix, so without this the
                // same value resolves differently per platform: `..\..\..\Windows\win.ini`
                // collapses and is refused on Windows, while on Linux nothing collapses, the whole thing
                // stays one long filename sitting INSIDE the root, and it is ACCEPTED. The node ships in
                // a Linux container and is developed on Windows, so the two must agree -- the same reason
                // the volume/UNC refusal above is written the way it is.
                //
                // Deliberately accepted consequence: a Unix file whose name genuinely contains a
                // backslash is now read as a nested path. That is pathological for a media store, and
                // platform-consistent refusal is worth more than supporting it.
                //
                // The TrimStart then drops leading separators so the value is always treated as
                // relative to the root.
                string normalized = userSuppliedPath.Replace('\\', '/').TrimStart('/');
                candidate = Path.GetFullPath(Path.Combine(resolvedRoot, normalized));
            }
            catch (Exception)
            {
                // Malformed path (invalid characters, too long, and so on). Refuse.
                return false;
            }

            StringComparison comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            // Trailing separator on the root before comparing, so "/rootevil" cannot pass as "/root".
            string rootWithSeparator = resolvedRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), comparison)
                ? resolvedRoot
                : resolvedRoot + Path.DirectorySeparatorChar;

            if (!candidate.StartsWith(rootWithSeparator, comparison))
            {
                return false;
            }

            fullPath = candidate;
            return true;
        }
    }
}
