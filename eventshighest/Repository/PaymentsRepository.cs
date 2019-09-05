using Dapper;
using eventshighest.Interface;
using eventshighest.Model;
using eventshighest.ViewModel;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class PaymentsRepository : BaseRepository,IpaymentsRepository
    {
        public readonly IConfiguration _config;
        public PaymentsRepository(IConfiguration config) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
        }
        public async Task<int> AddPayoutAccount(BankAccount bankAccount,int userid)
        {
           const string Sql = "insert into BankAccounts(Userid,account_number,account_bank,name)values(@userid,@accountno,@bankcode,@name)";
           return await WithConnection(async c =>
            {
              return  await c.ExecuteAsync(Sql,
                new
                {
                    @userid = userid,
                    @accountno = bankAccount.account_number,
                    @bankcode = bankAccount.code,
                    @name = bankAccount.name,
                }
                , commandType: CommandType.Text);
            });
        }
        public async Task<int> Requestwithdrawalpayment(WithdrawalviewModel withdrawalviewModel,int userid)
        {
            return await WithConnection(async c =>
            {
                return await c.ExecuteAsync("Withdrwalrequestv2",
                  new
                  {
                      @userid = userid,
                      @requestedwithdrawalamount = withdrawalviewModel.Amount,
                      @bankaccountid = withdrawalviewModel.Accountid,
                      @currencycode = withdrawalviewModel.Currency,
                  }
                  , commandType: CommandType.StoredProcedure);
            });
        }
        public async Task<IEnumerable<BankAccount>> Getuserbankaccounts(int userid)
        {
            const string Sql = "select * from BankAccounts where Userid = @userid";
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<BankAccount>(Sql,
                  new
                  {
                      @userid = userid
                  }
                  , commandType: CommandType.Text);
            });
        }
    }
}
