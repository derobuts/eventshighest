using Dapper;
using eventshighest.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Repository
{
    public class SearchRepository : BaseRepository,ISearchRepository
    {
        public readonly IConfiguration _config;
        public SearchRepository(IConfiguration config) : base(config["Dbconstring:dbConnectionString"])
        {
            _config = config;
        }
        public async Task<IEnumerable<dynamic>> GetByName(string searchword)
        {
            return await WithConnection(async c =>
            {
                var sqlparams = new DynamicParameters();
                sqlparams.Add("@word", searchword, DbType.String);
                //sqlparams.Add("@lastrecordno", lastrecordno, DbType.Int32);
                //sqlparams.Add("@noofrowsreturn", noofrowsreturn, DbType.Int32);
                //sqlparams.Add("@maxid", DbType.Int32, direction: ParameterDirection.Output);
                return await c.QueryAsync<dynamic>("SearchactivitybyName"
                    , sqlparams,
                    commandType: CommandType.StoredProcedure
                    );
                // int maxid = Events.Max(X => X.Eventid);
            });
        }
    }
}
