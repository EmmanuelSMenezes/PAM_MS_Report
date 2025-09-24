using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class FiltersReports
    {
        public Guid? Partner_id { get; set; }
        public string Start_date { get; set; }
        public string End_date { get; set; }
    }
}
