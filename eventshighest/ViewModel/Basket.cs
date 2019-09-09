using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class Basket
    {
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
        public string BuyerId { get; set; }
        public decimal Total()
        {
            return Math.Round(Items.Sum(x => x.UnitPrice * x.Quantity), 2);
        }
    }
}
