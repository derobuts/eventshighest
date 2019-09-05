using eventshighest.Interface;
using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Service
{
    public class Venueservice : IVenue
    {
        private readonly IActivityRepository _activityRepository;
        
        public Task<int> Addvenue(Venue venue)
        {
            return Task.FromResult(1);
        }
    }
}
