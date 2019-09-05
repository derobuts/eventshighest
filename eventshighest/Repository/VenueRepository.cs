using Dapper;
using eventshighest.Interface;
using eventshighest.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class VenueRepository : BaseRepository, IVenueRepository
    {
        public readonly IConfiguration _config;
        public VenueRepository(IConfiguration config) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
        }
        public async Task<int>Addactivityvenue(Venue venue)
        {         
            return await WithConnection<int>(async c =>
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@latitude", venue.latitude, DbType.Decimal);
                sqlparams.Add("@longitude", venue.longitude, DbType.Decimal);
                sqlparams.Add("@city",venue.city, DbType.String);
                sqlparams.Add("@country", venue.country, DbType.String);
                sqlparams.Add("@timezone", venue.timezone, DbType.String);
                sqlparams.Add("@placeaddress", venue.placeaddress, DbType.String);
                sqlparams.Add("@output",DbType.Int32, direction: ParameterDirection.Output);
                await c.ExecuteAsync("AddVenue", sqlparams, commandType: CommandType.StoredProcedure);
                int OutPut = sqlparams.Get<int>("@output");
                return OutPut;
            }
             );
        }
        //get event venue by name
        public async Task<IEnumerable<T>> GetVenueByName<T>(string searchword, int lastrecordno, int noofrowsreturn)
        {
            return await WithConnection(async c =>
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@word", searchword, DbType.String);
                sqlparams.Add("@lastrecordno", lastrecordno, DbType.Int32);
                sqlparams.Add("@noofrowsreturn", noofrowsreturn, DbType.Int32);
                var venuesearch = await c.QueryAsync<T>("SearchVenueName"
                    , sqlparams,
                    commandType: CommandType.StoredProcedure
                    );
                return venuesearch;
            });
        }

        public async Task<IEnumerable<dynamic>>Popularcities()
        {
            return await WithConnection(async c =>
            {
                string sql = @"select top 8 V.City,count(a.Activity_id)as Activities
                             from Venue v inner join Activity a on a.Venue_id = v.Venue_id
                             group by v.City
                             order by Activities desc";
                return await c.QueryAsync<dynamic>(sql, commandType: CommandType.Text);
            });
        }
        public async Task<IEnumerable<dynamic>>ActivitiesinCity(string cityname,int pagesize,int pagenumber)
        {
            return await WithConnection(async c =>
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@PageSize",pagesize, DbType.Int32);
                parameters.Add("@PageNumber",pagenumber, DbType.Int32);
                parameters.Add("@Cityname",cityname, DbType.String);
                return await c.QueryAsync<dynamic>("Gettopactivitescity",parameters,commandType: CommandType.StoredProcedure);
            });
        }
    }
}
