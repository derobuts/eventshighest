using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class Exchangerates
    {
        public string Basecurrency { get; set; }
        public string Tocurrencyid { get; set; }
        public decimal Rate { get; set; }
    }
}
