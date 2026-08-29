// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.EnumLibrary
{
    /// <summary>
    /// Affiliate types recognized by <c>FebrisOrAffiliateAuthorizeAttribute</c>.
    /// These are the non-Febris partner orgs that have their own user-management
    /// flows alongside (and visible to) the central Febris staff.
    /// </summary>
    public enum AffiliateType
    {
        ContentDeveloper,
        AccreditationBody,
    }
}
