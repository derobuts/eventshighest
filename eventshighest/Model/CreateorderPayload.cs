using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class CreateorderPayload
    {
        public int Activityid { get; set; }
        public DateTime Activityoccurrencedate { get; set; }
        public List<Orderitem> Ordertoreserve { get; set; }
    }
}
