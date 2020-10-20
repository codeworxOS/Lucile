using System;
using System.ComponentModel.DataAnnotations;

namespace Lucile.Test.Model
{
    public class ArticleSettings : EntityBase
    {
        public Guid Id { get; set; }

        [MaxLength(40)]
        public string Whatever { get; set; }
    }
}