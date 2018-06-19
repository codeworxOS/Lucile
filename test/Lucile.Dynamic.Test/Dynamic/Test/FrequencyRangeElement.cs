using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Lucile.Dynamic.Test.Dynamic.Test
{
    [DataContract]
    public partial class FrequencyRangeElement : ValidationBase, IHasIdentifier
    {
    }
}