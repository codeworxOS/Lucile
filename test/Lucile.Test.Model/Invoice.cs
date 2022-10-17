using System;

namespace Lucile.Test.Model
{
    public class Invoice : PayableReceipt
    {
        public DateTime? ExpectedDeliveryDate { get; set; }
    }
}