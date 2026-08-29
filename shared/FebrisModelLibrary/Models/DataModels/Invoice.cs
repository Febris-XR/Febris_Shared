// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.Models.DataModels
{
    #region Base Classes
    public class BaseInvoiceClass:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }
        public DateTime PaybyDate { get; set; }
        //public string InvoiceNumber { get; set; }
        public string ItemNumber { get; set; }

        public string OrderId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string AccountNumber { get; set; }
        public bool IsPaid { get; set; }


        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal NewSubtotal { get; set; }
        //public decimal TaxRate { get; set; }
        //public decimal TaxTotal { get; set; }
        public decimal Total { get; set; }
        public string Notes { get; set; }

        /// <summary>
        /// Routing who the invoice belongs to
        /// </summary>    
        /// 
        #region routing
        public InvoiceAccountType PayerAccountType { get; set; }        
        public InvoiceAccountType PayeeAccountType { get; set; }

        #region payer
        //public License PayerLicense { get; set; }
        //public Guid PayerLicenseUUID { get; set; }

        //public ContentDeveloper PayerContentDeveloper { get; set; }
        //public Guid PayerContentDeveloperUUID { get; set; }

        //public AccreditationBody PayerAccreditationBody { get; set; }
        //public Guid PayerAccreditationBodyUUID { get; set; }
        #endregion


        #region payee
        //public License PayeeLicense { get; set; }
        //public Guid PayeeLicenseUUID { get; set; }

        //public ContentDeveloper PayeeContentDeveloper { get; set; }
        //public Guid PayeeContentDeveloperUUID { get; set; }

        //public AccreditationBody PayeeAccreditationBody { get; set; }
        //public Guid PayeeAccreditationBodyUUID { get; set; }
        #endregion
        #endregion

    }
    public class Invoice : BaseInvoiceClass
    {
        public decimal TaxRate { get; set; }
        public decimal TaxTotal { get; set; }

        // Added by migration 20250819005016_InvoiceUpdates -- partial-payment ledger.
        public decimal AmountPaid { get; set; }
        public decimal CurrentBalanceOutstanding { get; set; }


        #region payer
        public License PayerLicense { get; set; }
        public Guid PayerLicenseUUID { get; set; }

        public ContentDeveloper PayerContentDeveloper { get; set; }
        public Guid PayerContentDeveloperUUID { get; set; }

        public AccreditationBody PayerAccreditationBody { get; set; }
        public Guid PayerAccreditationBodyUUID { get; set; }
        #endregion
    }
    public class Disbursement : BaseInvoiceClass
    {
        public decimal ServiceChargeRate { get; set; } = 30;//StaticDetails.ServiceChargeRate;
        public decimal ServiceCharge { get; set; }

        //public decimal TotalAfterServiceCharge { get; set; }

        #region payee
        public License PayeeLicense { get; set; }
        public Guid PayeeLicenseUUID { get; set; }

        public ContentDeveloper PayeeContentDeveloper { get; set; }
        public Guid PayeeContentDeveloperUUID { get; set; }

        public AccreditationBody PayeeAccreditationBody { get; set; }
        public Guid PayeeAccreditationBodyUUID { get; set; }
        #endregion
    }
    #endregion

    #region Item classes
    public class BaseInvoiceItemClass:BaseModel
    {
        //public long Id { get; set; }
        //public Guid UUID { get; set; }
        //public DateTime TimeStamp { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public Guid SerialNumber { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int Discount { get; set; }
        public decimal Subtotal { get; set; }

        //invoice
        //public bool HasBeenInvoiced { get; set; }
        public InvoiceAccountType PayerAccountType { get; set; }
        public Invoice Invoice { get; set; }
        public Guid InvoiceUUID { get; set; }

        //dispursement
        //public bool HasBeenDispursed { get; set; }
        public InvoiceAccountType PayeeAccountType { get; set; }
        public Disbursement Disbursement { get; set; }
        public Guid DisbursementUUID { get; set; }

        //need this so they can easily be sorted out
        public MarketplaceListing MarketplaceListingReference { get; set; }
        public Guid MarketplaceListingReferenceUUID { get; set; }
    }  
  
    public class InvoiceItem : BaseInvoiceItemClass
    {
        //public Invoice Invoice { get; set; }
        //public Guid InvoiceUUID { get; set; }


    }

    //public class DisbursementItem : BaseInvoiceItemClass
    //{
    //    //public Disbursement Disbursement { get; set; }
    //    //public Guid DisbursementUUID { get; set; }
    //    //public bool IsInvoice { get; set; }
    //}
    #endregion

    #region view models    
    public class InvoiceCreationViewModel
    {
        public decimal TaxRate { get; set; }
        public decimal TaxTotal { get; set; }
        public InvoiceCreationViewModel()
        {
            BaseInvoiceClass = new BaseInvoiceClass()
            {
                //DueDate = DateTime.UtcNow.AddDays(30),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow
            };
            InvoiceItemList = new List<InvoiceItem>();
        }
        public BaseInvoiceClass BaseInvoiceClass { get; set; }
        public List<InvoiceItem> InvoiceItemList { get; set; }

        
        public InvoiceAccountType PayeeAccountType { get; set; }
        public SelectList PayeeSelectList { get; set; }
        public Guid SelectedPayee { get; set; }

        public InvoiceAccountType PayerAccountType { get; set; }
        public SelectList PayerSelectList { get; set; }
        public Guid SelectedPayer { get; set; }
    }

    public class InvoiceCreationSetupViewModel
    {        
        //type of account of payer
        public InvoiceAccountType PayerAccountType { get; set; }
        //type of account of payee
        public InvoiceAccountType PayeeAccountType { get; set; }
    }

    #endregion



}
