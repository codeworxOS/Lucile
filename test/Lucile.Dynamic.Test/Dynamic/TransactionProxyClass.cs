using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.Dynamic
{
    public class TransactionProxyClass
    {
        public TransactionProxyClass()
        {
            this.Targets = new Collection<TestTarget>();
        }

        public virtual ICollection<TestTarget> Targets { get; set; }

        public virtual int IntProperty { get; set; }

        public virtual string StringProperty { get; set; }

        public virtual decimal DecimalProperty { get; set; }

    }
}
