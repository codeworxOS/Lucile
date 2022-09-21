using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Core.Test.Model.Source
{
    public class Address
    {
        public Address()
        {
            this.Invoices = new List<Invoice>();
        }

        public int Id { get; set; }

        public string City { get; set; }

        public Country Country { get; set; }
        public List<Invoice> Invoices { get; }
    }
}
