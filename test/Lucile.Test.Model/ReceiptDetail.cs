using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.Test.Model
{
    public class ReceiptDetail
    {
        public decimal Amount { get; set; }

        public Article Article { get; set; }

        public Guid? ArticleId { get; set; }

        public DateTime DeliveryTime { get; set; }

        public string Description { get; set; }

        public bool Enabled { get; set; }

        public Guid Id { get; set; }

        public decimal Price { get; set; }

        public Receipt Receipt { get; set; }

        public Guid ReceiptId { get; set; }
    }
}