// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    public class InvoicePaymentRequest
    {
        [Required]
        public Guid InvoiceUUID { get; set; }
        public Guid LicenseKey { get; set; }

        /// <summary>
        /// What stripe Needs
        /// </summary>
        /// 
        [Required]
        [Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Required]
        [Display(Name = "Credit Card Number")]
        [DataType(DataType.CreditCard)]
        public string CCNumber { get; set; }
        [Required]
        [Display(Name = "Expiration Date")]
        [DisplayFormat(DataFormatString = "{0:MMM-yyyy}")]
        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; } = DateTime.Now;
        [Required]
        [Display(Name = "CVC")]
        public string CVC { get; set; }
        [Required]
        [Display(Name = "Name On Card")]
        public string NameOnCard { get; set; }
        [Required]
        [Display(Name = "Zip Code")]
        public string ZIP { get; set; }
        //public CountryCode CountryCode { get; set; }

        // Payment-flow controls used by InvoiceLogic.PayInvoice / PaymentAPIRequest:
        //   PayTotal=true  -> charge the full outstanding balance (PartialPayment ignored).
        //   PayTotal=false -> charge the PartialPayment amount (capped at outstanding balance).
        public bool PayTotal { get; set; }
        public decimal PartialPayment { get; set; }

        // StripeInt carries the gateway-side material (public key, service-charge rate,
        // payment-method/token) that the front-end Stripe.js stripe.confirmCardPayment()
        // flow needs. Populated by GetInvoicePaymentRequest BLL; consumed by the Razor
        // payment-form partial.
        public StripeInt StripeInt { get; set; }
    }
    public class InvoicePaymentResponse
    {
        public Guid InvoiceUUID { get; set; }
        public long InvoiceNumber { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal FinalPayment { get; set; }
    }

   
}
