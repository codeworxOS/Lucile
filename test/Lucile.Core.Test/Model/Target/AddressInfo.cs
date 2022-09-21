using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucile.Core.Test.Model.Target
{
    public class AddressInfo
    {
        public int Id { get; set; }

        public string City { get; set; }

        public CountryInfo Country { get; set; }

        public IEnumerable<InvoiceInfo> Invoices { get; set; }
    }
}
