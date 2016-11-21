using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.Test.Model
{
    public class Invoice : Receipt
    {
        public DateTime LastPaymentDate { get; set; }
    }
}
