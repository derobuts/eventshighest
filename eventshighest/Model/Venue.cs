using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class Venue
    {
        public int id { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string placeaddress { get; set; }
        public bool venueispresent { get; set; }
        public string timezone { get; set; }
    }
}
