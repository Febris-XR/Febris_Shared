// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System.Collections.Generic;

namespace Febris.ModelLibrary.ApiContracts
{
    /// <summary>
    /// Generic paged API result. The wire contract between a subsystem's API and the portals that
    /// consume it (for example the SSO identity-audit read API -> the AdminPortal staff view), so a
    /// portal can render a page plus its pager without referencing the owning subsystem's BLL/DAL.
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
