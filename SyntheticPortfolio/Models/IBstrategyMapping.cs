using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyntheticPortfolio.Models
{
    public class IBstrategyMapping
    {
        public string TickerName { get; set; }
        public string AccountName { get; set; }
        public string IBStrategy { get; set; }
        public System.DateTime LastUpdated { get; set; }
    }
}