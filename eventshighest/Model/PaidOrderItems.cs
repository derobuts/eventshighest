using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class PaidOrderItems
    {
            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Barcode")]
            public string Barcode { get; set; }

            [JsonProperty("Barcodeimgbase64")]
            public string Barcodeimgbase64 { get; set; }

            [JsonProperty("Startdate")]
            public string Startdate { get; set; }

            [JsonProperty("Enddate")]
            public DateTimeOffset Enddate { get; set; }

            [JsonProperty("PlaceAddress")]
            public string PlaceAddress { get; set; }
        }
    }

