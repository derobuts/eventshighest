using Dapper;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class OrderRepository : BaseRepository,IOrderRepository
    {
        public readonly IConfiguration _config;
        public OrderRepository(IConfiguration config) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
        }
        public async Task<int>CreateOrder(int userid,CreateorderPayload createorderPayload)
        {
            List<Task> addticketstoorder = new List<Task>();
            var order = new Order();
            order.Orderitems = createorderPayload.Ordertoreserve;
            var occurrencedateintid = await WithConnection(async c =>
            {
                              
                return c.Query<int>("select Activity_occurrence_id from Activity_occurrence where Activity_id = @activityid and Start_datetime = @activitydate",
                                                  new { activityid = createorderPayload.Activityid, @activitydate = createorderPayload.Activityoccurrencedate }, commandType: CommandType.Text).FirstOrDefault();                              
            });
            var orderid = await WithConnection(async c =>
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@activityoccurrenceid",occurrencedateintid,DbType.Int32);
                parameters.Add("@userid",userid,DbType.Int32);
                parameters.Add("@amount", order.Orderamount, DbType.Int32);
                parameters.Add("@output", DbType.Int32, direction: ParameterDirection.Output);
                await c.ExecuteAsync("Createorder",parameters, commandType: CommandType.StoredProcedure);
                int output = parameters.Get<int>("@output");
                return output;
            });          
            foreach (var ticket in createorderPayload.Ordertoreserve)
            {
                ICurrency currency = null;
                await new TicketRepository(_config).AddTickettoorder(occurrencedateintid, orderid, ticket.Id, ticket.Quantity);
            }
            //await Task.WhenAll(addticketstoorder);
            return orderid;
        }
        public async Task Updateorderaddress(int orderid)
        {

        }
        public async Task<Order> Getorder(int orderid)
        {
            var order = new Order();
            order.Orderid = orderid;
            order.Orderitems = await WithConnection(async c =>
            {
                string Sql = @"select (select Name from Ticketclass where Ticket_id = tc.ticketclassid) as ticketclassname,(select Price from Ticketclass where Ticket_id = tc.ticketclassid)as ticketprice,tc.ticketsselected as ticketsselected
                from(
                select tc.Ticket_id as ticketclassid, count(t.Activity_ticket_id) as ticketsselected
                from Orders o
                inner join Ticket t on t.Orderid = o.OrdersId
                inner join Ticketclass tc on tc.Ticket_id = t.Ticketclass_id
                where o.OrdersId = @orderid
                group by tc.Ticket_id
                )tc";
                return await c.QueryAsync<Orderitem>(Sql, new { @orderid = orderid});
            });
            return order;
        }
        
        public async Task<IEnumerable<dynamic>> Getorders(int activityid, DateTime startdatetime, DateTime enddatetime,string Search,int PageSize, int PageNo,int PaymentStatus)
        {
            DynamicParameters parameters = new DynamicParameters();
            string Sql;
            if (PaymentStatus == 0)
            {
                if (Search == null)
                {
                    Sql = @"select OT.OrdersId as Id,concat(OT.Amount,' ',c.Code) as Amount,u.Email,case when ot.Status = 101 THEN 'Completed'
            when ot.Status = 100 and ot.Expiryts > GETUTCDATE() then 'Pending'
            when ot.Status = 100 OR ot.Status = 102 OR ot.Expiryts  < GETUTCDATE()  then 'Failed'
            end as Status,tx.Code as TransactionCode
            from Orders OT    
			inner join AspNetUsers u on u.Id = OT.Userid
            inner join PaymentStatus Ps on Ps.StatusId = OT.Status
            inner join TransactionHistory tx on tx.Order_Id = ot.OrdersId
			inner join Currency c on c.Codeid = tx.Currency
            where Activity_occurrence in (select Activity_occurrence_id from Activity_occurrence where Activity_id = @activityid)  and OT.Created >= @startdatetime and OT.Created < @enddatetime
            order by OT.Created desc  OFFSET @pagesize * (@pageno - 1) ROW FETCH NEXT @pagesize ROWS ONLY";
                    parameters.Add("@activityid", activityid, DbType.Int32);
                    parameters.Add("@pagesize", PageSize, DbType.Int32);
                    parameters.Add("@pageno", PageNo, DbType.Int32);
                    parameters.Add("@startdatetime", startdatetime, DbType.DateTime2);
                    parameters.Add("@enddatetime", enddatetime, DbType.DateTime2);
                }
                else
                {
                    Sql = @"
			select OT.OrdersId as Id,concat(OT.Amount,' ',c.Code) as Amount,u.Email,case when ot.Status = 101 THEN 'Completed'
            when ot.Status = 100 and ot.Expiryts > GETUTCDATE() then 'Pending'
            when ot.Status = 100  and ot.Expiryts  < GETUTCDATE()  then 'Failed'
            end as Status,tx.Code as TransactionCode,ot.Created
            from Orders OT    
			inner join AspNetUsers u on u.Id = OT.Userid
            inner join PaymentStatus Ps on Ps.StatusId = OT.Status
            left join TransactionHistory tx on tx.Order_Id = ot.OrdersId
			inner join Currency c on c.Codeid = tx.Currency
            where Activity_occurrence in (select Activity_occurrence_id from Activity_occurrence where Activity_id = @activityid) AND TX.Code = @code and OT.Created >= @startdatetime and OT.Created < @enddatetime
			UNION ALL
			select OT.OrdersId as Id,concat(OT.Amount,' ',c.Code) as Amount,u.Email,case when ot.Status = 101 THEN 'Completed'
            when ot.Status = 100 and ot.Expiryts > GETUTCDATE() then 'Pending'
            when ot.Status = 100 and ot.Expiryts  < GETUTCDATE()  then 'Failed'
            end as Status,tx.Code as TransactionCode,ot.Created
            from Orders OT    
			inner join AspNetUsers u on u.Id = OT.Userid
            inner join PaymentStatus Ps on Ps.StatusId = OT.Status
            left join TransactionHistory tx on tx.Order_Id = ot.OrdersId
			inner join Currency c on c.Codeid = tx.Currency
            where Activity_occurrence in (select Activity_occurrence_id from Activity_occurrence where Activity_id = @activityid) AND u.Email = @email and OT.Created >= @startdatetime and OT.Created < @enddatetime
            order by OT.Created desc  OFFSET @pagesize * (@pageno - 1) ROW FETCH NEXT @pagesize ROWS ONLY";
                    parameters.Add("@activityid", activityid, DbType.Int32);
                    parameters.Add("@pagesize", PageSize, DbType.Int32);
                    parameters.Add("@pageno", PageNo, DbType.Int32);
                    parameters.Add("@startdatetime", startdatetime, DbType.DateTime2);
                    //parameters.Add("@orderid",int.Parse(Search), DbType.Int32);
                    parameters.Add("@code",Search, DbType.String);
                    parameters.Add("@email", Search, DbType.String);
                    //parameters.Add("@orderid", int.Parse(Search), DbType.Int32);
                    parameters.Add("@enddatetime", enddatetime, DbType.DateTime2);
                }
               
            }
            else
            {
                if (Search == null)
                {
                    string sql = @"select OT.OrdersId as Id,concat(OT.Amount,' ',c.Code) as Amount,u.Email,case when ot.Status = 101 THEN 'Completed'
            when ot.Status = 100 and ot.Expiryts > GETUTCDATE() then 'Pending'
            when ot.Status = 100 and ot.Expiryts  < GETUTCDATE()  then 'Failed'
            end as Status,tx.Code as TransactionCode
            from Orders OT    
			inner join AspNetUsers u on u.Id = OT.Userid
            inner join PaymentStatus Ps on Ps.StatusId = OT.Status
            left join TransactionHistory tx on tx.Order_Id = ot.OrdersId
			inner join Currency c on c.Codeid = tx.Currency
            where  Activity_occurrence in (select Activity_occurrence_id from Activity_occurrence where Activity_id = @activityid) 
            and OT.Created >= @startdatetime and OT.Created < @enddatetime";
                    StringBuilder stringBuilder = new StringBuilder(sql);
                    stringBuilder.Append(PaymentStatus == 101 ? " and OT.Status = 101" : PaymentStatus == 100 ? " and ot.Status = 100 and ot.Expiryts > GETUTCDATE()" : PaymentStatus == 102 ? " and ot.Status = 100 and ot.Expiryts  < GETUTCDATE()":"");
                    Sql = stringBuilder.ToString();
                    parameters.Add("@activityid", activityid, DbType.Int32);
                    parameters.Add("@pagesize", PageSize, DbType.Int32);
                    parameters.Add("@pageno", PageNo, DbType.Int32);
                    parameters.Add("@status", PaymentStatus, DbType.Int32);
                    parameters.Add("@startdatetime", startdatetime, DbType.DateTime2);
                    parameters.Add("@enddatetime", enddatetime, DbType.DateTime2);
                }
                else
                {
                    string sql1 = @"select OT.OrdersId as Id,concat(OT.Amount,' ',c.Code) as Amount,u.Email,case when ot.Status = 101 THEN 'Completed'
            when ot.Status = 100 and ot.Expiryts > GETUTCDATE() then 'Pending'
            when ot.Status = 100 and ot.Expiryts  < GETUTCDATE()  then 'Failed'
            end as Status,tx.Code as TransactionCode
            from Orders OT    
			inner join AspNetUsers u on u.Id = OT.Userid
            inner join PaymentStatus Ps on Ps.StatusId = OT.Status
            left join TransactionHistory tx on tx.Order_Id = ot.OrdersId
			inner join Currency c on c.Codeid = tx.Currency
            where  Activity_occurrence in (select Activity_occurrence_id from Activity_occurrence where Activity_id = @activityid) and u.Email = @email 
            and OT.Created >= @startdatetime and OT.Created < @enddatetime";
                    StringBuilder stringBuilder = new StringBuilder(sql1);
                    stringBuilder.Append(PaymentStatus == 101 ? " and OT.Status = 101" : PaymentStatus == 100 ? " and ot.Status = 100 and ot.Expiryts > GETUTCDATE()" : PaymentStatus == 102 ? " and ot.Status = 100 and ot.Expiryts  < GETUTCDATE()" : "");
                    sql1 = stringBuilder.ToString();
                    string sql2 = @"select OT.OrdersId as Id,concat(OT.Amount,' ',c.Code) as Amount,u.Email,case when ot.Status = 101 THEN 'Completed'
            when ot.Status = 100 and ot.Expiryts > GETUTCDATE() then 'Pending'
            when ot.Status = 100 and ot.Expiryts  < GETUTCDATE()  then 'Failed'
            end as Status,tx.Code as TransactionCode
            from Orders OT    
			inner join AspNetUsers u on u.Id = OT.Userid
            inner join PaymentStatus Ps on Ps.StatusId = OT.Status
            left join TransactionHistory tx on tx.Order_Id = ot.OrdersId
			inner join Currency c on c.Codeid = tx.Currency
            where  Activity_occurrence in (select Activity_occurrence_id from Activity_occurrence where Activity_id = @activityid) AND TX.Code = @code
            and OT.Created >= @startdatetime and OT.Created < @enddatetime";
                    StringBuilder stringBuilder2 = new StringBuilder(sql2);
                    stringBuilder2.Append(PaymentStatus == 101 ? " and OT.Status = 101" : PaymentStatus == 100 ? " and ot.Status = 100 and ot.Expiryts > GETUTCDATE()" : PaymentStatus == 102 ? " and ot.Status = 100 and ot.Expiryts  < GETUTCDATE()" : "");
                    sql2 = stringBuilder2.ToString();
                    Sql = stringBuilder.ToString() + " union all " + stringBuilder2.ToString();
                }
                parameters.Add("@activityid", activityid, DbType.Int32);
                parameters.Add("@pagesize", PageSize, DbType.Int32);
                parameters.Add("@pageno", PageNo, DbType.Int32);
                parameters.Add("@startdatetime", startdatetime, DbType.DateTime2);
                parameters.Add("@status", PaymentStatus, DbType.Int32);
                // parameters.Add("@orderid", int.Parse(Search), DbType.Int32);
                parameters.Add("@code", Search, DbType.String);
                parameters.Add("@email", Search, DbType.String);
                //parameters.Add("@orderid", int.Parse(Search), DbType.Int32);
                parameters.Add("@enddatetime", enddatetime, DbType.DateTime2);

            }
            return await WithConnection(async c =>
            {
                return await c.QueryAsync<dynamic>(Sql, parameters, commandType: CommandType.Text);
            });
        }

        public Task UpdateConfirmedOrder(int orderid)
        {
           


            throw new NotImplementedException();
        }
    }
}
    