using Dapper;
using eventshighest.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class DashboardRepository : BaseRepository,IDashboardRepository
    {
        public readonly IConfiguration _config;
        public DashboardRepository(IConfiguration config) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
        }
        public async Task<IEnumerable<dynamic>> Getuseractivities(int userid, int status)
        {
            return await WithConnection(async c =>
            {
                string SQl = status == 101 ? "select Activity_id,Name,photo from Activity where Tenant_Id = @tenantid and Enddate < getutcdate() order by Created_On desc" :
                                              "select Activity_id,Name,photo from Activity where Tenant_Id = @tenantid and Enddate > getutcdate() order by Created_On desc";
                return await c.QueryAsync<dynamic>(SQl,
                                                  new { @tenantid = userid }, commandType: CommandType.Text);
            });
        }
        public async Task<IEnumerable<dynamic>> GetActivitystats(int activityid)
        {
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<dynamic>("MonthlyStats",
                                                  new { @activitytid = activityid }, commandType: CommandType.StoredProcedure);
            });
        }
        public async Task<IEnumerable<dynamic>> Currencies()
        {
            return await WithConnection(async c =>
            {
                //string Sql = "select * from Currency"
                return await c.QueryAsync<dynamic>("select * from Currency", commandType: CommandType.Text);
            });
        }
        public async Task<IEnumerable<dynamic>>Getorders(int activityid,DateTime startdatetime,DateTime enddatetime,int PageSize,int PageNo)
        {
            
            string Sql = @"select OT.OrdersId as Id,au.Email,OT.Amount,(select Code from Currency where Codeid = tx.Currency) as Currency,case when ot.Status = 101 THEN 'Completed'
            when ot.Status = 100 and ot.Expiryts > GETUTCDATE() then 'Pending'
            when ot.Status = 100 and ot.Expiryts  < GETUTCDATE()  then 'Failed'
            end as Status
            from Orders OT
            inner join AspNetUsers au on au.Id = ot.OrdersId
            inner join PaymentStatus Ps on Ps.StatusId = OT.Status
            inner join TransactionHistory tx on tx.Order_Id = ot.OrdersId
            where Activity_occurrence in (select Activity_occurrence_id from Activity_occurrence where Activity_id = @activityid) and OT.Paidts >= @startdatetime and OT.Paidts < @enddatetime
            ";
            StringBuilder sbCommand = new StringBuilder(Sql);
            if (PageSize > 0)
            {
                sbCommand.Append(" order by OT.Created desc  OFFSET @pagesize * (@pageno - 1) ROW FETCH NEXT 1 ROWS ONLY");
            }
            return await WithConnection(async c =>
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@activityid", PageNo, DbType.Int32);
                parameters.Add("@pagesize",PageSize, DbType.Int32);
                parameters.Add("@pageno", PageNo, DbType.Int32);
                parameters.Add("@startdatetime",startdatetime, DbType.DateTime2);
                parameters.Add("@enddatetime",enddatetime, DbType.DateTime2);
                return await c.QueryAsync<dynamic>(sbCommand.ToString(),parameters,commandType: CommandType.StoredProcedure);
            });
        }
    
        public async Task<IEnumerable<dynamic>>UserBalance(int userid)
        {        
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<dynamic>("Getuserbalance",
                                                  new { @userid = userid }, commandType: CommandType.StoredProcedure);
            });
        }     
      
    }
}
