using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucile.Test.Model
{
    public class Order : Receipt
    {
        public Guid ResponsibleEmployeeId { get; set; }

        public Contact ResponsibleEmployee { get; set; }
    }
}
