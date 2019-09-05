using eventshighest.Interface;
using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Service
{
    public class Activityservice : IActivityService
    {
        private readonly IActivityRepository _activityRepository;
        public Activityservice(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
        }
        public async Task<IEnumerable<dynamic>>GetActivityCategories()
        {
           return await _activityRepository.GetActivityCategories();
        }


        public Task<dynamic> GetNearbyActivities(decimal latitude, decimal longitude, int category)
        {
            throw new NotImplementedException();
        }

        public async Task AddActivity(Activity activity, int userid)
        {
            await _activityRepository.AddActivity(activity,userid);
        }
    }
}
