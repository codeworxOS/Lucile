using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Codeworx.Data
{
    [DataContract(IsReference = true)]
    public class NavigationPropertyValues : EntityTypeObject
    {
        public NavigationPropertyValues()
        {
            this.Values = new Collection<NavigationPropertyValue>();
        }

        [DataMember]
        public string PropertyName { get; set; }

        [DataMember]
        public Collection<NavigationPropertyValue> Values { get; private set; }
    }
}
