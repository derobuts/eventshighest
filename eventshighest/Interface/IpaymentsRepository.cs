using eventshighest.Model;
using eventshighest.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IpaymentsRepository
    {
        Task<int> Requestwithdrawalpayment(WithdrawalviewModel withdrawalviewModel,int userid);
        Task<int> AddPayoutAccount(BankAccount bankAccount, int userid);
        Task<IEnumerable<BankAccount>> Getuserbankaccounts(int userid);
    }
}
