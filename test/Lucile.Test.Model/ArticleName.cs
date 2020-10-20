using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lucile.Test.Model
{
    public class ArticleName : EntityBase
    {
        public Article Article { get; set; }

        public Guid ArticleId { get; set; }

        [MaxLength(4)]
        [Required]
        public string LanguageId { get; set; }

        [MaxLength(255)]
        [DefaultValue("testchen")]
        public string TranlatedText { get; set; }
    }
}