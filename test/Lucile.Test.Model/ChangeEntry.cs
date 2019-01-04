using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lucile.Test.Model
{
    public abstract class ChangeEntry
    {
        public ChangeVersion ChangeVersion { get; set; }

        [Required]
        [StringLength(200)]
        public string Test { get; set; }

        public long Version { get; set; }
    }
}