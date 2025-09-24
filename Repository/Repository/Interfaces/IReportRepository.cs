using System;
using System.Collections.Generic;
using Domain.Model;

namespace Infrastructure.Repository
{
  public interface IReportRepository
    {
        Reports CreateReport(ReportRequest request);
        ListReportResponse GetReports(FilterPagination filterPagination);
        ReportResponse GetReportByReportId(Guid report_id);
        bool DeleteReportByReportId(List<Guid> report_id);
        Reports UpdateReport(Reports report);
        List<Orders> GetOrdersByPartner(FiltersReports filtersReports);
    }
}
