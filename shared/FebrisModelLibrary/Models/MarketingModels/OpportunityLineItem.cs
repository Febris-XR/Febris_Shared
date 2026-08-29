// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.ComponentModel.DataAnnotations;

namespace Febris.ModelLibrary.Models.MarketingModels
{
    /// <summary>
    /// CRM Phase 2 Tier 1 (2026-05-21): single line item on an
    /// <see cref="Opportunity"/>. Sums up to <c>Opportunity.Amount</c>.
    /// <para>
    /// Two ways to identify the product on a line:
    /// <list type="bullet">
    ///   <item>By Marketplace listing: set
    ///   <see cref="MarketplaceListingUUID"/>. The product name shown
    ///   to the rep is the linked listing's display name.</item>
    ///   <item>Free-text: leave <see cref="MarketplaceListingUUID"/>
    ///   null and set <see cref="ProductName"/> directly. Useful for
    ///   custom bundles, services, or pre-product-launch deals.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <c>LineTotal</c> = <c>Quantity * UnitPrice * (1 - DiscountPercent/100)</c>
    /// computed by <c>OpportunityLineItemLogic</c>; not persisted because
    /// the inputs are persisted (computed at read time / on rollup).
    /// </para>
    /// </summary>
    public class OpportunityLineItem : BaseModel
    {
        /// <summary>
        /// Parent <see cref="Opportunity"/>. Required.
        /// </summary>
        [Required]
        public Guid OpportunityUUID { get; set; }

        /// <summary>
        /// Optional link to a <see cref="DataModels.MarketplaceListing"/>.
        /// When set, the rep picked an existing product; otherwise the
        /// line is free-text via <see cref="ProductName"/>.
        /// </summary>
        public Guid? MarketplaceListingUUID { get; set; }

        /// <summary>
        /// Free-text product / service name. Populated either from the
        /// linked MarketplaceListing.Name (snapshotted at line creation
        /// so renaming a listing later doesn't rewrite history) or by
        /// the rep for free-text lines.
        /// </summary>
        [Required]
        [StringLength(200)]
        [Display(Name = "Product")]
        public string ProductName { get; set; }

        /// <summary>Short description shown beneath the product on the
        /// line item row. Optional.</summary>
        [StringLength(500)]
        public string Description { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Per-unit price BEFORE the line-item discount. Currency is
        /// inherited from the parent Opportunity.Currency.
        /// </summary>
        [Display(Name = "Unit price")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Per-line discount percent (0-100). Whole-opportunity
        /// discount (if added later) would be a separate field on
        /// Opportunity.
        /// </summary>
        [Range(0, 100)]
        [Display(Name = "Discount (%)")]
        public decimal DiscountPercent { get; set; }
    }
}
