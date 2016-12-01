using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.Test.Model
{
    public class ArticleName
    {
        public Article Article { get; set; }

        public Guid ArticleId { get; set; }

        [MaxLength(4)]
        [Required]
        public string LanguageId { get; set; }

        [MaxLength(255)]
        public string TranlatedText { get; set; }
    }
}