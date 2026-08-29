// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    /// <summary>
    /// Result of a video part-merge attempt.
    /// <para>
    /// T6. The merge used to return a bare bool that the caller discarded, and
    /// <c>ProcessVideoFiles</c> returned a hardcoded <c>true</c> regardless, so the node answered
    /// <c>200 {"Success":true}</c> whether it had assembled a recording, skipped the merge, or
    /// produced a truncated file. These cases have to be distinguishable before the response can be
    /// honest, because only two of them are failures.
    /// </para>
    /// </summary>
    public enum VideoMergeOutcome
    {
        /// <summary>
        /// A part was stored and the set is not complete yet. This is the NORMAL result for every
        /// part except the last, and it is a success from the client's point of view.
        /// </summary>
        PartAccepted = 1,

        /// <summary>All parts arrived and the recording was assembled and moved into place.</summary>
        Merged = 2,

        /// <summary>
        /// The merge was skipped because another merge holds this key. Legitimate under genuine
        /// concurrency, but it was ALSO what a key-mismatch leak produced permanently: the entry
        /// was added under the full split path and removed under the bare filename, so it was never
        /// removed and every later merge of that name was skipped while still reporting success.
        /// </summary>
        Skipped = 3,

        /// <summary>The merge ran and did not produce a usable recording.</summary>
        Failed = 4
    }
}
