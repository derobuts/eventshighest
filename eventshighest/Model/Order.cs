using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class Order
    {
        public int Orderid { get; set; }
        public IEnumerable<Orderitem> Orderitems { get; set; }
        public decimal Amount { get; set; }
        public decimal Orderamount
        {
            get
            {
                decimal amount = 0;
                foreach (var orderitem in Orderitems)
                {
                    amount += orderitem.Unitprice * orderitem.Quantity;
                }
                return amount;
            }
        }
    }
}
