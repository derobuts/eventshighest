using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IVenueRepository
    {
        Task<int> Addactivityvenue(Venue venue);
        Task<IEnumerable<dynamic>> Popularcities();
        Task<IEnumerable<dynamic>> ActivitiesinCity(string cityname, int pagesize, int pagenumber);
    }
}
