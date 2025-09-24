using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Domain.Model;

namespace Application.Service.Interfaces
{
    public interface IReportService
    {
        Reports CreateReport(ReportRequest request, string token);
        ListReportResponse GetReports(FilterPagination filterPagination);
        ReportResponse GetReportByReportId(Guid report_id);
        bool DeleteReportByReportId(List<Guid> report_id);
        Reports UpdateReport(Reports report, string token);
        byte[] GetReportByPartnerId(FiltersReports filtersReports);
    }
}
