using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Dapper;
using Domain.Model;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Npgsql;
using Serilog;

namespace Infrastructure.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        public ReportRepository(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public Reports CreateReport(ReportRequest request)
        {
            using (var connection =  new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {                    
                    var sql = @$"INSERT INTO report.report
                                (name, created_by, description)
                                VALUES('{request.Name}','{request.Created_by}', '{request.Description}') RETURNING *";
                    var inserted = connection.Query<Reports>(sql).FirstOrDefault();
                    List<Filter> filters = new List<Filter>();
                    foreach (var item in request.Filters)
                    {
                        var sqlFilter = @$"INSERT INTO report.report_filter
                                            (filter_name, created_by, report_id) 
                                            VALUES ('{item.Filter_name}', '{request.Created_by}', '{inserted.Report_id}') RETURNING *";
                        var insertedFilter = connection.Query<Filter>(sqlFilter).FirstOrDefault();
                        filters.Add(insertedFilter);
                    }
                    if (inserted != null || filters.Count() != request.Filters.Count())
                    {
                        throw new Exception("ErrorInsert");
                    }
                    transaction.Commit(); 
                    inserted.Filters = filters;
                    return inserted;
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    connection.Close();
                    _logger.Error(ex, "Exception [service] when creating report!");
                    throw ex;
                }

            }
        }

        public ListReportResponse GetReports(FilterPagination filterPagination)
        {
            try
            {
                var sql = @$"SELECT r.*,  (SELECT json_agg(filter) from
                            (select * 
                            FROM report.report_filter p
                            WHERE p.report_id = r.report_id) filter ) AS Filters FROM report.report r";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var response = connection.Query(sql).Select(x => new Reports()
                    {
                        Report_id = x.report_id,
                        Name= x.name,   
                        Description= x.description,
                        Active= x.active,
                        Created_at= x.created_at,
                        Created_by= x.created_by,
                        Updated_at= x.updated_at,
                        Updated_by= x.updated_by,
                        Filters= !string.IsNullOrEmpty(x.filters) ? JsonConvert.DeserializeObject<List<Filter>>(x.filters) : new List<Filter>(),
                    }).ToList();
                    if (response != null)
                    {
                        int totalRows = response.Count();
                        float totalPages = (float)totalRows / (float)filterPagination.ItensPerPage;
                        totalPages = (float)Math.Ceiling(totalPages);

                        return new ListReportResponse()
                        {
                            Reports = response,
                            Pagination = new Pagination()
                            {
                                totalPages = (int)totalPages,
                                totalRows = totalRows
                            }
                        };
                    };

                    return new ListReportResponse();    
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ReportResponse GetReportByReportId(Guid order_id)
        {
            try
            {
                string sql = @$"SELECT r.*,  (SELECT json_agg(filter) from
                            (select p.filter_name 
                            FROM report.report_filter p
                            WHERE p.report_id = r.report_id) filter ) AS Filters FROM report.report r WHERE r.report_id = '{order_id}'";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var response = connection.Query(sql).Select(x => new ReportResponse()
                    {
                        Report_id = x.report_id,
                        Name = x.name,
                        Description = x.description,
                        Active = x.active,
                        Created_at = x.created_at,
                        Created_by = x.created_by,
                        Updated_at = x.updated_at,
                        Updated_by = x.updated_by,
                        Filters = !string.IsNullOrEmpty(x.filters) ? JsonConvert.DeserializeObject<List<Filter>>(x.filters) : new List<Filter>(),
                    }).ToList();

                    return response.FirstOrDefault();


                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public Reports UpdateReport(Reports report)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    string sql = $@"UPDATE report.report SET 
                                           name = '{report.Name}'
                                         , description = '{report.Description}'
                                         , updated_by = '{report.Updated_by}'
                                         , updated_at = now()
                                        WHERE report_id = '{report.Report_id}' RETURNING *";

                    var updated = connection.Query<Reports>(sql).FirstOrDefault();

                    List<Filter> filters = new List<Filter>();
                    foreach (var item in report.Filters)
                    {
                        var sqlFilter = @$"UPDATE report.report_filter SET 
                                                filter_name = '{item.Filter_name}', updated_by = '{report.Updated_by}', updated_at = now() WHERE report_filter_id = '{item.Report_filter_id}' RETURNING *";
                        var updatedFilter = connection.Query<Filter>(sqlFilter).FirstOrDefault();
                        filters.Add(updatedFilter);
                    }

                    if (updated == null || filters.Count() != report.Filters.Count())
                    {
                        transaction.Dispose();
                        connection.Close();
                        throw new Exception("errorWhileUpdateReport");
                    }

                    transaction.Commit();
                    connection.Close();

                    updated.Filters = filters;
                    return updated;
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    connection.Close();
                    throw ex;
                }
            }
        }

        public bool DeleteReportByReportId(List<Guid> report_id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();

                    foreach (var item in report_id)
                    {                        
                        string queryReportId = $@"SELECT * FROM report.report WHERE report_id = '{report_id}'";
                        var reportId = connection.Query<ReportResponse>(queryReportId).FirstOrDefault();

                        if (reportId != null)
                        {
                            string sqlDeleteReport = $@"DELETE FROM report.report WHERE report_id = '{reportId.Report_id}' RETURNING *";
                            connection.Execute(sqlDeleteReport);

                            ///string sqlDeleteFilter = $@"DELETE FROM report.report_filter WHERE report_id = '{reportId.Report_id}' RETURNING *";
                            ///connection.Execute(sqlDeleteFilter);

                            transaction.Commit();
                            connection.Close();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                throw new Exception("errorWhileDeleteReport");
            }
        }

        public List<Orders> GetOrdersByPartner(FiltersReports filtersReports)
        {
            try
            {
                string sql = @$"select o.order_number, o.amount, o.created_at, o.service_fee,o.card_fee, o.amount *(o.service_fee+o.card_fee) fee, p.legal_name  
                                from orders.orders o
                                inner join partner.branch b on b.branch_id = o.branch_id 
                                inner join partner.partner p on p.partner_id =b.partner_id 
                                where p.partner_id = '{filtersReports.Partner_id}' 
                                and o.created_at between '{filtersReports.Start_date}' and '{filtersReports.End_date}' and o.order_status_id ='e04621d0-3997-4c69-9054-a10257602a29'
                                order by o.order_number";

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    var response = connection.Query<Orders>(sql).ToList();

                    return response;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
