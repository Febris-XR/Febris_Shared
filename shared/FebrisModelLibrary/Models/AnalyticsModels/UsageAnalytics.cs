// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.Models.AnalyticsModels
{
    public class SoftwareDownloadAnalytics: AnalyticsBaseModel
    {
        public long? LicenseId { get; set; }
        public Guid? LicenseUUID { get; set; }

        public long? HardwareId { get; set; }
        public Guid? HardwareUUID { get; set; }

        public long? ContentDeveloperId { get; set; }
        public Guid? ContentDeveloperUUID { get; set; }

        public long? AccreditationBodyId { get; set; }
        public Guid? AccreditationBodyUUID { get; set; }

        public Guid? UserId { get; set; }

        public bool? FebrisUser { get; set; }

        [Display(Name = "Downloaded package")]
        public long? LocalSoftwarePackageId { get; set; }
        public Guid? LocalSoftwarePackageUUID { get; set; }

    }

    public class ModuleDownloadAnalytics : AnalyticsBaseModel
    {
        public long? LicenseId { get; set; }
        public Guid? LicenseUUID { get; set; }

        public long? HardwareId { get; set; }
        public Guid? HardwareUUID { get; set; }

        public long? ContentDeveloperId { get; set; }
        public Guid? ContentDeveloperUUID { get; set; }

        public long? AccreditationBodyId { get; set; }
        public Guid? AccreditationBodyUUID { get; set; }

        public Guid? UserId { get; set; }

        public bool? FebrisUser { get; set; }


        public long? ModuleId { get; set; }
        public Guid? ModuleUUID { get; set; }
    }

    public class ModuleUsageAnalytics : AnalyticsBaseModel
    {
        public long? LicenseId { get; set; }
        public Guid? LicenseUUID { get; set; }

        public long? HardwareId { get; set; }
        public Guid? HardwareUUID { get; set; }

        public long? ContentDeveloperId { get; set; }
        public Guid? ContentDeveloperUUID { get; set; }

        public long? AccreditationBodyId { get; set; }
        public Guid? AccreditationBodyUUID { get; set; }

        public Guid? UserId { get; set; }
        public bool? FebrisUser { get; set; }


        public long? ModuleId { get; set; }
        public Guid? ModuleUUID { get; set; }
    }
}
