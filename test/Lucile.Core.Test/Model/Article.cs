using System;

namespace Lucile.Core.Test.Model
{
    public class Article
    {
        public Guid Id { get; set; }

        public string ArticleNumber { get; set; }

        public string ArticleDescription { get; set; }

        public decimal Price { get; set; }

        public Guid SupplierId { get; set; }

        public Contact Supplier { get; set; }
    }
}