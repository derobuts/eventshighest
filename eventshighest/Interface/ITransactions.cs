using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface ITransactions
    {
        Task<IEnumerable<Transactions>> GetTransactions(int userid, DateTime Startdate, DateTime Enddate, int? Type, string Status, int PageNo, int pageSize, string Query);
        Task WithdrawlRequest(int id, decimal amount);
        Task<dynamic> UserBalanceNetPayouts(int userid, string currency);
    }
}
