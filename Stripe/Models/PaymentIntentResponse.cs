using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Stripe.Models
{
    public class PaymentIntentResponse
    {
        public long Amount { get; set; }
        public string Currency { get; set; }
        public List<ChargeData> Charges { get; set; }
        public AutomaticTax AutomaticTax { get; set; }
    }

    public class AutomaticTax
    {
        public bool Enabled { get; set; }
        public long AmountCollected { get; set; }
        public List<TaxAmount> TaxAmounts { get; set; }
    }

    public class ChargeData
    {
        public string Id { get; set; }
        public long Amount { get; set; }
        public string Currency { get; set; }
        public List<TaxAmount> TaxAmounts { get; set; }
        public string BalanceTransactionId { get; set; } // Adjusted to match actual API response
    }

    public class TaxAmount
    {
        public long Amount { get; set; }
        public decimal Rate { get; set; }
        public string Type { get; set; }
    }

    public class BalanceTransaction
    {
        public List<FeeDetail> FeeDetails { get; set; }
    }

    public class FeeDetail
    {
        public long Amount { get; set; }
    }
}