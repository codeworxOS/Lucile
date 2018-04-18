using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    [DataContract]
    public class SampleFault
    {
        [DataMember]
        public string Text { get; set; }
    }
}