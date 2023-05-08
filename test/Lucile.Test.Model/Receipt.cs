using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Lucile.Test.Model
{
    [KnownType(typeof(Order))]
    [KnownType(nameof(GetSubTypes))]
    public abstract class Receipt : EntityBase
    {
        public Receipt()
        {
            this.Details = new List<ReceiptDetail>();
        }

        public Contact Customer { get; set; }

        public Guid CustomerId { get; set; }

        public List<ReceiptDetail> Details { get; set; }

        public Guid Id { get; set; }

        public DateTime ReceiptDate { get; set; }

        [StringLength(100)]
        public string ReceiptNumber { get; set; }

        public ReceiptType ReceiptType { get; set; }

        public static IEnumerable<Type> GetSubTypes()
        {
            yield return typeof(PayableReceipt);
        }
    }
}