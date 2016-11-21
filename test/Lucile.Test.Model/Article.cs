using System;

namespace Lucile.Test.Model
{
    public class Article
    {
        public string ArticleDescription { get; set; }

        public string ArticleNumber { get; set; }

        public ArticleSettings ArticleSettings { get; set; }

        public Guid Id { get; set; }

        public decimal Price { get; set; }

        public Contact Supplier { get; set; }

        public Guid SupplierId { get; set; }
    }
}