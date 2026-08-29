// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    /// <summary>Lifecycle states an invite can be in when looked up by
    /// the accept page. Active is the only state where the user is
    /// allowed to consume; everything else is a friendly-error path.
    /// <para>
    /// Shared by the central developer-org invite flow and the user node's
    /// own invite flow. <c>OrgNotReady</c> is central-only (a node has no
    /// org to be un-ready) and <c>Revoked</c> is node-only today. Both
    /// callers switch with a default branch, so neither is broken by the
    /// other's member.
    /// </para></summary>
    public enum InviteState
    {
        Active,
        NotFound,
        Expired,
        AlreadyConsumed,
        OrgNotReady,

        /// <summary>An administrator cancelled the invite before it was used.
        /// Distinct from <see cref="Expired"/>, which happens on its own, and
        /// worth distinguishing because a revoked invite means somebody decided
        /// this person should not get an account. Appended LAST deliberately:
        /// the central flow persists no ordinal of this enum, but appending
        /// costs nothing and removes the question.</summary>
        Revoked
    }
}
