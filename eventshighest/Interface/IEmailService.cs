using eventshighest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface IEmailService
    {
        Task sendEmail(CancellationToken cancellationToken);
        Task<bool> SendEmailAsync(string emailto, string subject, object message, string templateid = null, string attachmentsfile = null);
        //void SetEmailpayoad(Paidorder paidorder);
    }
}
