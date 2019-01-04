using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lucile.Test.Model
{
    public class ChangeVersion
    {
        [Key]
        public long Version { get; set; }
    }
}