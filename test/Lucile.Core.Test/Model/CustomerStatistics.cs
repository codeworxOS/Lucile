using System;

namespace Tests
{
    public class CustomerStatistics
    {
        public Guid CustomerId { get; set; }

        public DateTime LastPurchase { get; set; }

        public decimal ReceiptAmountCurrentYear { get; set; }

        public decimal ReceiptAmountCurrentMonth { get; set; }

        public decimal ReceiptAmountLastMonth { get; set; }
    }
}