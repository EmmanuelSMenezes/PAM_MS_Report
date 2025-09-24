using FastReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class Orders
    {
        public int Order_number { get; set; }
        public decimal Amount { get; set; }
        public DateTime Created_at { get; set; }
        public decimal Service_fee { get; set; }
        public decimal Card_fee { get; set; }
        public decimal Fee { get; set; }
        public string Legal_name { get; set; }
    }
}
