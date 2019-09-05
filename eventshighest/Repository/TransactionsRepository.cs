using Dapper;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class TransactionsRepository : BaseRepository, ITransactions
    {
        public readonly IConfiguration _config;
        public TransactionsRepository(IConfiguration config) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
        }
        public async Task<IEnumerable<Transactions>> GetTransactions(int userid, DateTime Startdate, DateTime Enddate, int? Type, string Status, int PageNo, int pageSize,string Query)
        { 
            return await WithConnection(async c =>
            {
                string Sql = @"select tx.Code,ROUND(Amount,2)as Amount,c.Code as Currency,txo.Description as Direction,ps.Status,Date,Narration
                             from TransactionHistory tx inner
                             join Currency c on c.Codeid = tx.Currency
                             inner
                             join PaymentStatus ps on ps.StatusId = tx.PaymentStatus
                             inner join TransactionOperation txo on txo.Id = tx.Txoperation
                             where tx.Userid = @userid";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@userid", userid, DbType.Int32);
                StringBuilder sbCommand = new StringBuilder(Sql);
                if (Type != null)
                {
                    sbCommand.Append(" and Txoperation = @txoperation");
                    parameters.Add("@txoperation", Type.Value,DbType.Int32);
                }
                if (Status != null)
                {
                    sbCommand.Append(" and ps.Status = @status");
                    parameters.Add("@status", Status, DbType.String);
                }
                if (Startdate != null)
                {
                    sbCommand.Append(" and tx.Date >= @startdate");
                    parameters.Add("@startdate",Startdate == DateTime.MinValue ? DateTime.UtcNow.AddMonths(-3) : Startdate, DbType.DateTime2);
                }
                if (Enddate != null)
                {
                    sbCommand.Append(" and tx.Date <= @enddate");
                    parameters.Add("@enddate", Enddate == DateTime.MinValue ? DateTime.UtcNow : Enddate, DbType.DateTime2);
                }
                if (Query != null)
                {
                    sbCommand.Append(" and tx.Code = @txcode");
                    parameters.Add("@txcode", Query, DbType.String);
                }
                sbCommand.Append(@" order by tx.Date desc
                             OFFSET @pagesize * (@pageno - 1) ROWS
                             FETCH NEXT @pagesize ROWS ONLY");
                parameters.Add("@pagesize",pageSize, DbType.Int32);
                parameters.Add("@pageno",PageNo, DbType.Int32);
                var h = sbCommand.ToString();
                return await c.QueryAsync<Transactions>(sbCommand.ToString(),parameters,commandType: CommandType.Text);              
            });
        }

        public Task<IEnumerable<Transactions>> GetPayoutAmount(int userid)
        {
            return WithConnection(async c =>
            {
                string sql = "select sum(Amount) as Payouts from TransactionHistory where Userid = @userid and Txoperation = @txoperation";
                return await c.QueryAsync<Transactions>(sql, new { @userid = userid, @txoperation  = 101})
                ;
            });
        }

        public async Task<dynamic> UserBalanceNetPayouts(int userid,string currency)
        {
            string Sql = @"
                         select isnull(sum(x.Grossincome * crd.Rate),0) as Grossincome,isnull(sum(x.Netincome * crd.Rate),0)as Netincome,isnull(sum(x.Balance * crd.Rate),0) as Balance,isnull(sum(x.Withdrawals * crd.Rate),0)as Withdrawals
                         from
                         (
                         select isnull(CASE WHEN Tx.Txoperation = 100 then(tx.Amount / cr.Rate)end, 0) as Grossincome,
                         isnull(CASE WHEN Tx.Txoperation = 100 then(tx.Amount / cr.Rate) * 100 / 110 end, 0) as Netincome,
                         isnull(CASE WHEN Tx.Txoperation = 100 then(tx.Amount / cr.Rate) * 100 / 110 when tx.Txoperation = 101 then(tx.Amount / cr.Rate) * -1 end, 0) as Balance,
                         isnull(CASE WHEN Tx.Txoperation = 101 then(tx.Amount / cr.Rate) end, 0) as Withdrawals,tx.Date,cr.Basecurrencyid
                         from TransactionHistory tx right join Currencyrates cr on tx.Currency = cr.Tocurrencyid
                         where tx.Userid = @userid and tx.PaymentStatus != 102 and tx.Date >= cr.Date_from and tx.Date < cr.Date_to
                         ) x
                         cross apply(
                         select cr.Rate,c.Code as Currency from Currencyrates cr
						 inner join Currency c on c.Codeid = cr.Tocurrencyid
                         where cr.Basecurrencyid = x.Basecurrencyid and x.Date >= cr.Date_from and x.Date < cr.Date_to
                         and cr.Tocurrencyid = (select Codeid from Currency where Code = @currency)
                         ) crd";
            return await WithConnection(async c =>
            {
                return c.Query<dynamic>(Sql,new { @userid = userid,@currency = currency }, commandType: CommandType.Text).FirstOrDefault();
            });
        }

        public Task AddUnverifiedTx(WebHookPayLoad payLoad)
        {
            return WithConnection2(async c =>
            {
                int txoperation = payLoad.EventType is null || payLoad.EventType != "Transfer" ? 100 : 101;
                string sql = "INSERT INTO Transactiontx (Code,Amount,Txoperation,PaymentStatus,Date,Currency)values(@txref,@txamount,@txoperation,@txpaymentstatus,@date,@currency)";
                await c.ExecuteAsync(sql, new { @txref = payLoad.txRef, @txamount = payLoad.amount, @txoperation = txoperation, @txpaymentstatus = 103, @date = payLoad.createdAt, @currency = payLoad.currency })
                ;
            });
        }
        public Task WithdrawlRequest(int id, decimal amount)
        {
            return WithConnection2(async c =>
            {
                string sql = "INSERT INTO Transactiontx (Code,Amount,Txoperation,PaymentStatus,Date,Currency)values(@txref,@txamount,@txoperation,@txpaymentstatus,@date,@currency)";
                await c.ExecuteAsync(sql, new { @txref = "NotConfirmed", @txamount = amount, @txoperation = 101, @txpaymentstatus = 103, @date = DateTime.UtcNow, @currency = "Unknown" })
                ;
            });
        }      
    }
}
