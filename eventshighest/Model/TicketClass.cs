using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class TicketClass
    {
        public string name { get; set; }
        public int max_per_order { get; set; }
        public decimal amount { get; set; }
        public int type { get; set; }
        public string feestype { get; set; }
        public decimal netamount { get; set; }
        public string status { get; set; }
        public string additionalinfo { get; set; }
        public decimal fees { get; set; }
        public int ticketstosell { get; set; }
        public string startsale { get; set; }
        public string endsale { get; set; }
        public int min_per_order { get; set; }
        public int visibility { get; set; }
        public string password { get; set; }
    }
}
