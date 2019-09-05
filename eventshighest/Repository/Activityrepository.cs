using Cronos;
using Dapper;
using eventshighest.Interface;
using eventshighest.Model;
using eventshighest.Repository;
using eventshighest.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace eventshighest.Controllers.Repository
{
    public class ActivityRepository : BaseRepository, IActivityRepository
    {
        public readonly IConfiguration _config;

        public ActivityRepository(IConfiguration config) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
        }
        async Task<IEnumerable<dynamic>> IActivityRepository.GetActivityCategories()
        {
           return await WithConnection(async c =>
           {
               string Sql = "select * from Activitytype";
               return await c.QueryAsync<dynamic>(Sql
                   ,
                   commandType: CommandType.Text
                   );
           });
        }

         

        public async Task<dynamic> Getactivitydetails(int activityid)
        {
            return await WithConnection(async c =>
            {
                string Sql = @"select Activity_id,Name,description,photo,v.PlaceAddress,v.Timezone from Activity a
                inner join Venue v on v.Venue_id = a.Venue_id
                where a.Activity_id = @activityid";
                 var h = await c.QueryAsync<dynamic>(Sql, new {@activityid = activityid}
                    ,
                    commandType: CommandType.Text
                    );
                return h.FirstOrDefault();
            });
        }
        public async Task<IEnumerable<dynamic>>PopularNearby(string country, string currency)
        {
            return await WithConnection(async c =>
            {
                var args = new DynamicParameters();
                args.Add("@country", country, dbType: DbType.String);
                args.Add("@currency",currency, dbType: DbType.String);
                
                return await c.QueryAsync<dynamic>("Gettopactivitesnearby"
                    , args,
                    commandType: CommandType.StoredProcedure
                    );
            });
        }

        public async Task<IEnumerable<dynamic>> Gettopactivities(string currencycode)
        {
            return await WithConnection(async c =>
            {
                var args = new DynamicParameters();
                args.Add("@currency",currencycode, dbType: DbType.String);
                return await c.QueryAsync<dynamic>("Gettopactivites"
                    , args,
                    commandType: CommandType.StoredProcedure
                    );
            });
        }
        public async Task<IEnumerable<T>> SearchactivityByName<T>(string searchword)
        {
            return await WithConnection(async c =>
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@word", searchword, DbType.String);
                //sqlparams.Add("@lastrecordno", lastrecordno, DbType.Int32);
                //sqlparams.Add("@noofrowsreturn", noofrowsreturn, DbType.Int32);
                //sqlparams.Add("@maxid", DbType.Int32, direction: ParameterDirection.Output);
                var Events = await c.QueryAsync<T>("SearchactivitybyName"
                    , sqlparams,
                    commandType: CommandType.StoredProcedure
                    );
                // int maxid = Events.Max(X => X.Eventid);
                return Events;
            });
        }
        public async Task<IEnumerable<dynamic>> ActivityCategories()
        {
            return await WithConnection(async c =>
            {
               return await c.QueryAsync<dynamic>(@"select * from Activitytype", CommandType.Text);
            });
        }
        public async Task<IEnumerable<DateTime>> Getactivityavailabledates(int activityid)
        {
            return await WithConnection(async c =>
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@activityid", activityid, DbType.Int64);
                return await c.QueryAsync<DateTime>("Getactivitydates",parameters,commandType:CommandType.StoredProcedure);
            });
        }
        public async Task AddActivity(Activity activity,int userid)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var venueid = activity.venue.venueispresent == true ? activity.venue.id : await new VenueRepository(_config).Addactivityvenue(activity.venue);
                   
                    int Activityid = await WithConnection(async c =>
                    {
                        var sqlparams = new DynamicParameters();
                        sqlparams.Add("@userid",userid, DbType.Int32);
                        sqlparams.Add("@name", activity.name, DbType.String);
                        sqlparams.Add("@activitytypeid", activity.activitytype, DbType.Int32);
                        sqlparams.Add("@photo", activity.photo, DbType.String);
                        sqlparams.Add("@description", activity.description, DbType.String);
                        sqlparams.Add("@startdate", activity.startdate, DbType.DateTime);
                        sqlparams.Add("@enddate", activity.enddate, DbType.DateTime);
                        sqlparams.Add("@venueid", venueid, DbType.Int32);
                        sqlparams.Add("@output", DbType.Int32, direction: ParameterDirection.Output);
                        await c.ExecuteAsync("AddActivity",
                        sqlparams,
                        commandType: CommandType.StoredProcedure);
                        int activityid = sqlparams.Get<int>("@output");
                        return activityid;
                    });
                        foreach (var ticketclass in activity.ticketClasses)
                        {
                            ICurrency currency = null;
                            await new TicketRepository(_config).AddTicketClass(Activityid, ticketclass);
                        }
                    await Addactivityoccurrences(activity, Activityid);
                    scope.Complete();
                }
            }
            catch (Exception ex)
            { 
                var h = ex;
                throw;
            }
        }
        public async Task Addactivityoccurrences(Activity activity,int activityid)
        {
            using (var c = new SqlConnection(_config["Dbconstring:dbConnectionString"]))
            {
                SqlCommand cmd = new SqlCommand("spaddactivityoccurrencesdates", c);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@ActivityoccurrencesdatesType";
                param.Value = GetActivityoccurrencesdates(activity, activityid);
                cmd.Parameters.Add(param);
                c.Open();
                await cmd.ExecuteNonQueryAsync();
                c.Close();
            }
        }
        /**only populate two months worth of data, add the rest through background tasks**/
        public DataTable GetActivityoccurrencesdates(Activity activity, int activityid)
        {
            DataTable dt = new DataTable();
            if (activity.recurring == 1)
            {
                CronExpression expression = CronExpression.Parse(activity.recurringpatterns[0].recurringstring);
                var occurrences = expression.GetOccurrences(DateTime.UtcNow, activity.enddate > DateTime.UtcNow.AddMonths(2) ? DateTime.UtcNow.AddMonths(2) : activity.enddate, fromInclusive: false, toInclusive: true);
                dt.Columns.Add("Activity_id");
                dt.Columns.Add("Start_datetime");
                dt.Columns.Add("End_datetime");
                foreach (var date in occurrences)
                {
                    DataRow row = dt.NewRow();
                    row["Activity_id"] = activityid;
                    row["Start_datetime"] = date;
                    row["End_datetime"] = date.AddMinutes(activity.recurringpatterns[0].intervallengthmins);
                    dt.Rows.Add(row);
                }
                return dt;
            }
            dt.Columns.Add("Activity_id");
            dt.Columns.Add("Start_datetime");
            dt.Columns.Add("End_datetime");
            DataRow rows = dt.NewRow();
            rows["Activity_id"] = activityid;
            rows["Start_datetime"] = activity.startdate;
            rows["End_datetime"] = activity.enddate;
            dt.Rows.Add(rows);
            return dt;
        }

       
    }
    }
