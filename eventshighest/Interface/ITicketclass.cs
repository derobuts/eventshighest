using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
   public interface ITicketclass
    {
        Task<IEnumerable<Ticketsubinfo>> GetactivityticketClasses(int activityid,string currency);
        Task<dynamic> Getactivitydateticketstatus(int activityid, DateTime activitydate, string currency);
    }
}
