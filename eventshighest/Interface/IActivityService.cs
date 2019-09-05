using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IActivityService
    {
       Task<dynamic> GetNearbyActivities(decimal latitude, decimal longitude, int category);
       Task<IEnumerable<dynamic>> GetActivityCategories();
       Task AddActivity(Activity activity,int userid);
    }
}
