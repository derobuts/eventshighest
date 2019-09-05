using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eventshighest.Model
{
    public class UrlQueryTx
    {
        private const int maxPageSize = 100;
        public bool IncludeCount { get; set; } = false;
        public string Status { get; set; }
        public string Query { get; set; }
        public DateTime Startdate { get; set; }
        public int? Type { get; set; }
        public DateTime Enddate { get; set; }
        public int? PageNumber { get; set; }

        private int _pageSize = 25;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value < maxPageSize) ? value : maxPageSize;
            }
        }
    }
}

