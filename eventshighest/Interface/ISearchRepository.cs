using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Interface
{
    public interface ISearchRepository
    {
        Task<IEnumerable<dynamic>> GetByName(string searchword);
    }
}
