using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Lucile.Dynamic.Test.Dynamic.Test
{
    [DataContract]
    public partial class AvdMask : ValidationBase, IHasIdentifier
    {
        #region Constructors

        public AvdMask()
        {
            this.Elements = new HashSet<FrequencyRangeElement>();
        }

        #endregion Constructors
    }
}