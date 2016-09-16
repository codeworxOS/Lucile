using System;
using System.Collections.Generic;

namespace Lucile.Core.Test.Model
{
    public class Receipt
    {
        public Guid CustomerId { get; set; }

        public Contact Customer { get; set; }

        public Guid Id { get; set; }

        public string ReceiptNumber { get; set; }

        public DateTime ReceiptDate { get; set; }

        public List<ReceiptDetail> Details { get; set; }
    }
}