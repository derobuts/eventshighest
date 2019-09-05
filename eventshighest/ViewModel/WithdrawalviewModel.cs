using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.ViewModel
{
    public class WithdrawalviewModel
    {
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "accountid")]
        public int Accountid { get; set; }
       // public int Paymentmethod { get; set; }
        public string Currency { get; set; }
    }
}
