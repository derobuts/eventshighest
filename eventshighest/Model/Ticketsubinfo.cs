using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class Ticketsubinfo
    {
        public int Ticketclassid { get; set; }
        public int Activity_id { get; set; }
        public string Name { get; set; }       
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string Ticket_type { get; set; }
        public int Minqtperorder { get; set; }
        public int Maxqtperorder { get; set; }

    }
}
