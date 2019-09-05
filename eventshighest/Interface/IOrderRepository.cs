using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IOrderRepository
    {
        Task<int> CreateOrder(int userid, CreateorderPayload createorderPayload);
        Task<IEnumerable<dynamic>> Getorders(int activityid, DateTime startdatetime, DateTime enddatetime, string Search, int PageSize, int PageNo,int PaymentStatus);
        Task<Order> Getorder(int orderid);
        Task Updateorderaddress(int orderid);
        Task UpdateConfirmedOrder(int orderid);
    }
}
