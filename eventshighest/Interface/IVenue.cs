using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IVenue
    {
        Task<int> Addvenue(Venue venue);
    }
}
