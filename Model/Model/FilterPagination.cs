using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class FilterPagination
    {
        public string Filter { get; set; }  
        public int Page { get; set; }   
        public int ItensPerPage { get; set; }   
    }
}
