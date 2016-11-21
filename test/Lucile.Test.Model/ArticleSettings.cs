using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.Test.Model
{
    public class ArticleSettings
    {
        public Guid Id { get; set; }

        [MaxLength(40)]
        public string Whatever { get; set; }
    }
}