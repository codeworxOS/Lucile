using System;
using System.ComponentModel.DataAnnotations;

namespace Lucile.Test.Model
{
    public class ArticleNameHistory : EntityBase
    {
        public Guid Id { get; set; }

        [MaxLength(4)]
        [Required]
        public string LanguageId { get; set; }

        public Guid ArticleId { get; set; }

        public ArticleName ArticleName { get; set; }

        public DateTime ChangeDate { get; set; }
    }
}