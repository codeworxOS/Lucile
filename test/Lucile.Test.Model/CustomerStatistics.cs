using System;
using Lucile.Test.Model;

namespace Tests
{
    public class CustomerStatistics : EntityBase
    {
        public Guid CustomerId { get; set; }

        public DateTime LastPurchase { get; set; }

        public decimal ReceiptAmountCurrentMonth { get; set; }

        public decimal ReceiptAmountCurrentYear { get; set; }

        public decimal ReceiptAmountLastMonth { get; set; }
    }
}