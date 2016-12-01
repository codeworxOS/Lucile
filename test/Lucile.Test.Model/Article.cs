using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lucile.Test.Model
{
    public class Article
    {
        public Article()
        {
            Names = new HashSet<ArticleName>();
        }

        public string ArticleDescription { get; set; }

        public string ArticleNumber { get; set; }

        public ArticleSettings ArticleSettings { get; set; }

        public string CurrentName
        {
            get
            {
                return Names?.FirstOrDefault(p => p.LanguageId == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)?.TranlatedText ?? ArticleDescription;
            }
        }

        public Guid Id { get; set; }

        public ICollection<ArticleName> Names { get; set; }

        public decimal Price { get; set; }

        public Contact Supplier { get; set; }

        public Guid SupplierId { get; set; }
    }
}