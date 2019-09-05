using Dapper;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class TicketRepository : BaseRepository,ITicketclass
    {
        public readonly IConfiguration _config;
        public readonly ICurrency _currency;
        public TicketRepository(IConfiguration config) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
        }
        public async Task<IEnumerable<Ticketsubinfo>> GetactivityticketClasses(int activityid,string Currency)
        {
            return await WithConnection(async c =>
            {
                string sql = @"select Name,crd.Rate * Price as Price,crd.Currency from
                (
                Select Basecurrencyid,tc.Startsale,tc.Endsale,tc.Name,tc.Currency,tc.Activity_id,(Price/cr.Rate) as Price
                From Ticketclass tc
                Inner join Activity a on a.Activity_id = tc.Activity_id
                Inner Join Currencyrates cr on cr.Tocurrencyid = tc.Currency 
                where tc.Activity_id = 2032 and tc.Startsale >= cr.Date_from and tc.Startsale < cr.Date_to
                ) as x 
                cross apply(
                select cr.Rate,(select Code from Currency where Codeid = cr.Tocurrencyid)as Currency
                from Currencyrates cr 
                         where cr.Basecurrencyid = x.Basecurrencyid and x.Startsale >= cr.Date_from and x.Startsale < cr.Date_to
                         and cr.Tocurrencyid = (select Codeid from Currency where Code = @currency)
                         ) crd ";
                return await c.QueryAsync<Ticketsubinfo>(sql, new {@activityid = activityid,@currency = Currency == null?"USD":Currency},
                    commandType: CommandType.Text
                    );
            });
        }

        public async Task<dynamic> Getactivitydateticketstatus(int activityid,DateTime activitydate,string currency)
        {         
            return await WithConnection(async c =>
            {
                string SQL = $@"select tc.Ticket_id as ticketclassid,tc.Name,tc.Price * y.Rate/cr.Rate as Price,tc.Maxqtperorder,tc.Minqtperorder,
                (case when tc.Tickets_to_sell = ISNULL(x.ticketsbought,0) then 'True' else 'False' end)as Issoldout,
                (case when tc.Startsale > GETUTCDATE()  then 'False' else 'True' end) as Salestarted,
                (case when tc.Endsale >= GETUTCDATE()  then 'False' else 'True' end) as Salesended
                from Activity a 
                inner join Activity_occurrence ao on ao.Activity_id = a.Activity_id
                inner join Ticketclass tc on tc.Activity_id = a.Activity_id
					Inner join 
                (
				select *,ROW_NUMBER() over (partition by Tocurrencyid order by Date_from desc) rowno 
				from Currencyrates 
				) as cr on cr.Tocurrencyid = tc.Currency and cr.rowno = 1
				Inner join
				(
				select *,ROW_NUMBER() over (partition by Tocurrencyid order by Date_from desc) rowno 
				from Currencyrates 
				) as y on  y.rowno = cr.rowno
                and y.Tocurrencyid = (select Codeid from Currency where Code = @currencycode )
                left join
                (
                select o.Activity_occurrence,count(*) as ticketsbought from Orders o 
                inner join Ticket t on o.OrdersId = t.Orderid
                group by o.Activity_occurrence
                ) as X on X.Activity_occurrence = ao.Activity_occurrence_id
                where a.Activity_id = @activityid and ao.Start_datetime = @activitydate";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@activityid",activityid,DbType.Int32);
                parameters.Add("@activitydate",activitydate,DbType.DateTime2);
                parameters.Add("@currencycode",currency, DbType.String);
                return await c.QueryAsync<dynamic>(SQL, new { @activityid = activityid, @activitydate = activitydate, @currencycode = currency }
                   ,
                   commandType: CommandType.Text
                   );
            });
        }
        public async Task AddTicketClass(int activityid, TicketClass ticketClass)
        {
            string Sql = "insert into TicketClass(Activity_id,Name,Price,Fee_id,Tickets_to_sell,additional_info,Currency,Minqtperorder,Maxqtperorder,Startsale,Endsale)" +
                         "values(@activityid,@name,@price,@feeid,@ticketstosell,@additionalinfo,@currency,@minperorder,@maxperorder,@startsale,@endsale)";
            await WithConnection2(async c =>
            { 
                await c.ExecuteAsync(Sql,
                new { @activityid = activityid,@name = ticketClass.name,@price = ticketClass.amount,@feeid = ticketClass.feestype,@ticketstosell = ticketClass.ticketstosell
                ,@additionalinfo = ticketClass.additionalinfo,@currency = 1,@minperorder = ticketClass.min_per_order,@maxperorder = ticketClass.max_per_order,@startsale = ticketClass.startsale,@endsale = ticketClass.endsale }
                , commandType: CommandType.Text);
            }
        );
        }
        public async Task AddTickettoorder(int activityoccurrenceid,int orderid,int ticketclassid,int quantitytoreserve)
        {
            await WithConnection2(async c =>
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@activityoccurrenceid",activityoccurrenceid, DbType.Int32);
                parameters.Add("@orderid",orderid, DbType.Int32);
                parameters.Add("@ticketclassid",ticketclassid,DbType.Int32);
                parameters.Add("@quantitytoreserve",quantitytoreserve, DbType.Int32);
                await c.ExecuteAsync("AddTickets",parameters,commandType: CommandType.StoredProcedure);
            }
        );
        }
    }
}
