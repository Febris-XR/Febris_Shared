// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// Company-level CRM record. Aggregates multiple <see cref="Lead"/>s
    /// belonging to the same organization so an enterprise B2B sales
    /// motion can track a buying committee across 3-5 stakeholders.
    /// <para>
    /// Distinct from <see cref="DataModels.ContentDeveloper"/> and
    /// <see cref="DataModels.AccreditationBody"/>, which are *operational*
    /// org records used by the platform's product surfaces (LMS, marketplace).
    /// Account is purely a CRM grouping -- it can exist before the company
    /// has any platform account, and it persists after the relationship
    /// converts to a paying customer.
    /// </para>
    /// <para>
    /// Auto-created opportunistically from <see cref="LeadInbox"/> intake
    /// (match by email domain or company name + country). Free-webmail
    /// domains do not auto-create Accounts -- those leads stay
    /// account-less until triaged.
    /// </para>
    /// </summary>
    public class Account : BaseModel
    {
        // ---- Identity ----

        [Required]
        [Display(Name = "Account name")]
        public string Name { get; set; }

        /// <summary>
        /// Primary email domain (e.g. "acme.com"). Used for dedup +
        /// auto-association of incoming leads. Optional -- not every
        /// account has a single primary domain.
        /// </summary>
        [Display(Name = "Email domain")]
        public string Domain { get; set; }

        /// <summary>
        /// Company website. Distinct from Domain because some companies
        /// market under one domain and email under another.
        /// </summary>
        public string Website { get; set; }

        // ---- Classification ----

        /// <summary>
        /// CRM stage at the Account (company) level. Distinct from
        /// <see cref="LeadDetails.LifecycleStage"/> which tracks the
        /// individual contact's stage -- three contacts at Acme may each
        /// be at different stages while the company is "Customer".
        /// </summary>
        [Display(Name = "Lifecycle stage")]
        public LifecycleStage LifecycleStage { get; set; }

        /// <summary>
        /// True when this Account represents a content developer
        /// (publishing partner) rather than a buyer. Lets reporting
        /// filter the two funnels without needing separate tables.
        /// Set when the lead converts via the SSO ContentDeveloper
        /// self-signup flow or by a Febris admin during triage.
        /// </summary>
        public bool IsContentDeveloper { get; set; }

        /// <summary>
        /// Owning rep -- ApplicationUser.Id of the AE / CSM responsible
        /// for the relationship. Nullable until the account is claimed.
        /// </summary>
        [Display(Name = "Owner")]
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Free-text industry classification. Mirrors <see cref="LeadDetails.Industry"/>;
        /// duplicating here so we can roll up "all Healthcare accounts"
        /// without joining through every Lead.
        /// </summary>
        public string Industry { get; set; }

        /// <summary>Approximate employee count. Use Size buckets in UI
        /// (1-10, 11-50, 51-200, 201-1000, 1001+) but persist as int
        /// so analytic ranges stay flexible.</summary>
        [Display(Name = "Number of employees")]
        public int NumberOfEmployees { get; set; }

        /// <summary>Last-known annual revenue / company value indicator
        /// (USD). Used for deal-size sanity and pipeline weighting.</summary>
        [Display(Name = "Company value (USD)")]
        public decimal CompanyValue { get; set; }

        public string Description { get; set; }

        // ---- Address ----

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }

        // ---- Lifecycle metadata ----

        /// <summary>
        /// Set true when the account is closed-lost or otherwise should
        /// be excluded from default reporting. Keeps the row for history
        /// rather than deleting.
        /// </summary>
        public bool Archive { get; set; }

        /// <summary>
        /// Leads associated with this account. EF navigation property --
        /// populate via Include when needed; not always set.
        /// </summary>
        public List<Lead> Leads { get; set; }
    }
}
