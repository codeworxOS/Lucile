using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.Dynamic
{
    public class NonVirtualBaseProperties
    {
        public byte[] ByteArrayProperty { get; set; }

        public int IntProperty { get; set; }

        public string StringProperty { get; set; }
    }
}