using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.Test.Model
{
    public class ContactSettings
    {
        public Contact Contact { get; set; }

        public Guid Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string TestSetting { get; set; }

        [MaxLength(50)]
        public byte[] TestSettingBinary { get; set; }
    }
}