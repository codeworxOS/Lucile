using System;
using System.Collections.Generic;

namespace Lucile.Test.Model
{
    public abstract class Receipt : EntityBase
    {
        public Receipt()
        {
            this.Details = new List<ReceiptDetail>();
        }

        public Contact Customer { get; set; }

        public Guid CustomerId { get; set; }

        public List<ReceiptDetail> Details { get; set; }

        public Guid Id { get; set; }

        public DateTime ReceiptDate { get; set; }

        public string ReceiptNumber { get; set; }

        public ReceiptType ReceiptType { get; set; }
    }
}