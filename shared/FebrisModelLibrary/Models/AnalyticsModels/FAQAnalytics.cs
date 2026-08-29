// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;

namespace Febris.ModelLibrary.Models.AnalyticsModels
{
    /// <summary>
    /// Analytics row counting clicks on a public FAQ entry. One row per
    /// FAQ; <see cref="Clicks"/> is incremented by the marketing-site
    /// FAQ controller each time the entry is opened.
    /// <para>
    /// Schema from migration 20220214230325_Initial. Does NOT inherit
    /// from <c>BaseModel</c> because the migration shipped its own
    /// <c>TimeStamp</c> + <c>UpdateTimeStamp</c> columns (note: not
    /// <c>LastUpdateTimeStamp</c>), pre-dating the BaseModel convention.
    /// </para>
    /// </summary>
    public class FAQAnalytics
    {
        public long Id { get; set; }
        public Guid UUID { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime UpdateTimeStamp { get; set; }

        public long? FAQId { get; set; }
        public Guid FAQUUID { get; set; }
        public long Clicks { get; set; }
    }
}
