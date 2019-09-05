using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eventshighest.Service
{
    public interface IActivityRepository
    {
      Task<IEnumerable<dynamic>> GetActivityCategories();
      Task<IEnumerable<dynamic>> Gettopactivities(string currencycode);
      Task AddActivity(Activity activity,int userid);
      Task<dynamic> Getactivitydetails(int activityid);
     Task<IEnumerable<DateTime>>Getactivityavailabledates(int activityid);
      Task<IEnumerable<dynamic>> PopularNearby(string latitude,string longitude);
    }
}