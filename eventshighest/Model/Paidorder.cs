using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class Paidorder
    {
        public int OrdersId { get; set; }
        public string Email { get; set; }
        public IEnumerable<PaidOrderItems> orderItems { get; set; }
    }
}
