// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    /// <summary>
    /// Which deployment a FileServerHandler instance runs in. An EndUser deployment is
    /// host-scoped to its own areas and must never create central or adminportal directories.
    /// The canonical area declaration lives in StorageManifests (Febris.SharedServices.Storage);
    /// this enum is the bridge for the legacy FileInitalizer until call sites move to the key
    /// model (overhaul Phase 3).
    /// </summary>
    public enum FileServerHostRole
    {
        Central = 0,
        EndUser = 1,
    }
}
