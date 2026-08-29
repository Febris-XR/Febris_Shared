// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.ModelLibrary.ViewModels
{
    class PaymentGatewayModels
    {
    }


    public interface IAccount
    {

    }

    public class StripeAccount : IAccount
    {
        public string user_id { get; set; }
        public string account_id { get; set; }
        public decimal amount { get; set; }
        public bool livemode { get; set; }
        public string transaction_id { get; set; }
    }
    public class StripeTokenData
    {
        public string account_id { get; set; }
        public string access_token { get; set; }
        public string expires_in { get; set; }
    }

    public class StripePaymentRequest : IAccount
    {
        public string PaymentMethodId { get; set; }
        public string PaymentIntentId { get; set; }
        public InvoicePaymentRequest InvoicePaymentRequest { get; set; }
        public string InvoiceNumber { get; set; }
        public long InvoiceTotal { get; set; }
        public string PaymentMessage { get; set; }
        public string CustomerName { get; set; }

        // Stripe Source/payment-method token forwarded to Stripe Charge.Source.
        // Set by InvoiceLogic.PaymentAPIRequest from StripeInt.StripeToken.
        public string StripeToken { get; set; }

        // Gateway processing fee, set by InvoiceLogic before submitting the charge
        // so the recorded payment ledger reflects the fee deducted from AmountPaid.
        public decimal GatewayFee { get; set; }
    }

    // Carrier for Stripe gateway parameters surfaced through InvoicePaymentRequest.StripeInt.
    // Populated by InvoiceLogic.GetInvoicePaymentRequest with the public key + service-charge
    // rate; the Razor payment-form posts back with StripeToken once Stripe.js produces it.
    public class StripeInt
    {
        public string PublicKey { get; set; }
        public decimal ServiceChargeRate { get; set; }
        public string StripeToken { get; set; }
    }
}
