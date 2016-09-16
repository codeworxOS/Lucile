using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.Core.Test.Model
{
    public class ReceiptDetail
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public Guid? ArticleId { get; set; }

        public Article Article { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }

        public Guid ReceiptId { get; set; }

        public Receipt Receipt { get; set; }
    }
}
