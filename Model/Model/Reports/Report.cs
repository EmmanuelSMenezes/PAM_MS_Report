using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Reports
    {
        public Guid Report_id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public Guid? Updated_by { get; set; }

        public DateTime? Updated_at { get; set; }
        public bool Active { get; set; }

        public List<Filter> Filters { get; set; }

    }
}
