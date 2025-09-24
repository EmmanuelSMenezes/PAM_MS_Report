using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{ 
    public class ReportRequest
    {
        public string Name { get; set; }
        public string Description { get; set; } 
        [JsonIgnore]
        public Guid Created_by { get; set; }
        public List<Filter> Filters { get; set; }
    }
    public class Filter
    {
        public string Filter_name { get; set; }
        public Guid Report_filter_id { get; set; }  
    }
}
