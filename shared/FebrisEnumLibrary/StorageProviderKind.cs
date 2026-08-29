// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    /// <summary>
    /// Backend kind selected per deployment via the "Storage:Provider" config value.
    /// </summary>
    public enum StorageProviderKind
    {
        FileSystem = 0,
        S3 = 1,
    }
}
