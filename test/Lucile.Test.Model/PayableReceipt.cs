using System;
using System.Runtime.Serialization;

namespace Lucile.Test.Model
{
    [KnownType(typeof(Invoice))]
    public class PayableReceipt : Receipt
    {
        public DateTime LastPaymentDate { get; set; }
    }
}
