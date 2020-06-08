using System.ComponentModel.DataAnnotations;

namespace Lucile.Test.Model
{
    public class ChangeVersion
    {
        [Key]
        public long Version { get; set; }
    }
}