using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IDashboardRepository
    {
        Task<IEnumerable<dynamic>> Getuseractivities(int userid, int status);
        Task<IEnumerable<dynamic>> GetActivitystats(int activityid);
        Task<IEnumerable<dynamic>> Getorders(int activityid, DateTime startdatetime, DateTime enddatetime, int PageSize, int PageNo);
        Task<IEnumerable<dynamic>> Currencies();
        Task<IEnumerable<dynamic>> UserBalance(int userid);
    }
}
