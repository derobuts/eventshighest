using eventshighest.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IravePayments
    {
        Task VerifyPayments(CancellationToken cancellationToken);
        Func<CancellationToken, Task> GetFuncVerifyPayments(string txref);
        Task<IEnumerable<Bank>> BanksSupportingTransferinregion(string isocountrycode);
        Func<CancellationToken, Task> GetTransfersHook(Transfer _transferpayload);
        Task CreateTransferRecipientAccount(CreateTransferRecipient transferRecipient);
        Task AddTransferRecipientAccount(CreateTransferRecipient transferRecipient);
        Task Completedeventstopayout();
        // Task Payoutrequests();
        Func<CancellationToken, Task> Getwithdrawalrequests();
    }
}
