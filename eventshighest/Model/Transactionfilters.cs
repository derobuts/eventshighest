using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class Transactionfilters
    {
        public string Status { get; set; }
        public string Query { get; set; }
        public DateTime Startdate { get; set; }
        public int? Type { get; set; }
        public DateTime Enddate { get; set; }
    }
}
