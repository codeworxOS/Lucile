using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Codeworx.Data.Metadata;

namespace Codeworx.Data
{
    [DataContract(IsReference=true)]
    [KnownType(typeof(ManyToManyNavigationPropertyValue))]
    public class NavigationPropertyValue
    {
        [DataMember]
        public object Value { get; set; }
    }

    [DataContract(IsReference = true)]
    public class ManyToManyNavigationPropertyValue : NavigationPropertyValue
    {
        [DataMember]
        public EntityKey EntityKey { get; set; }
    }
}
