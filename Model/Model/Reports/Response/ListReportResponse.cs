﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class ListReportResponse
    {
        public List<Reports> Reports { get; set; }
        public Pagination Pagination { get; set; }
    }
}
