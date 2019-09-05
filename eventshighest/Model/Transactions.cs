using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class Transactions
    {
        [JsonProperty(PropertyName = "TxCode")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "Currency")]
        public string Currency { get; set; }
        [JsonProperty(PropertyName = "Direction")]
        public string Direction { get; set; }
        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "Date")]
        public string Date { get; set; }
        [JsonProperty(PropertyName = "Narration")]
        public string Narration { get; set; }
    }
}
