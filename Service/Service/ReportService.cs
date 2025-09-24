using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Application.Service.Interfaces;
using Domain.Model;
using FastReport.Data;
using FastReport;
using FastReport.Export.PdfSimple;
using FastReport.Web;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Data;

namespace Application.Service
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;
        private readonly ILogger _logger;
        private readonly string _privateSecretKey;
        private readonly string _tokenValidationMinutes;

        public ReportService(
          IReportRepository repository,
          ILogger logger,
          string privateSecretKey,
          string tokenValidationMinutes
          )
        {
            _repository = repository;
            _logger = logger;
            _privateSecretKey = privateSecretKey;
            _tokenValidationMinutes = tokenValidationMinutes;
        }

        public DecodedToken GetDecodeToken(string token, string secret)
        {
            DecodedToken decodedToken = new DecodedToken();
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            if (IsValidToken(token, secret))
            {
                foreach (Claim claim in jwtSecurityToken.Claims)
                {
                    if (claim.Type == "email")
                    {
                        decodedToken.Email = claim.Value;
                    }
                    else if (claim.Type == "name")
                    {
                        decodedToken.Name = claim.Value;
                    }
                    else if (claim.Type == "userId")
                    {
                        decodedToken.UserId = new Guid(claim.Value);
                    }
                    else if (claim.Type == "roleId")
                    {
                        decodedToken.RoleId = new Guid(claim.Value);
                    }
                }

                return decodedToken;
            }

            throw new Exception("invalidToken");
        }

        public bool IsValidToken(string token, string secret)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("emptyToken");
            }
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters();
            tokenValidationParameters.ValidateIssuer = false;
            tokenValidationParameters.ValidateAudience = false;
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Base64UrlEncoder.Encode(secret)));

            try
            {
                SecurityToken validatedToken;
                ClaimsPrincipal claimsPrincipal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public Reports CreateReport(ReportRequest request, string token)
        {
            try
            {
                var decodedToken = GetDecodeToken(token.Split(' ')[1], _privateSecretKey);
                if (decodedToken == null) throw new Exception("errorDecodingToken");
                request.Created_by = decodedToken.UserId;
                var response = _repository.CreateReport(request);
                if (response == null) throw new Exception("createError");
                return response;
            } 
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public ListReportResponse GetReports(FilterPagination filterPagination)
        {
            try
            {
                var response = _repository.GetReports(filterPagination);
                if (response == null) throw new Exception("createError");
                return response;
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
                var order = _repository.GetReportByReportId(order_id);
                if (order == null) throw new Exception("errorListingOrder");
                return order;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public Reports UpdateReport(Reports report, string token)
        {
            try
            {
                var decodedToken = GetDecodeToken(token.Split(' ')[1], _privateSecretKey);
                if (decodedToken == null) throw new Exception("errorDecodingToken");
                report.Updated_by = decodedToken.UserId;
                var response = _repository.UpdateReport(report);
                if (response == null) throw new Exception("createError");
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool DeleteReportByReportId(List<Guid> report_id)
        {
            try
            {
                var response = _repository.DeleteReportByReportId(report_id);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public byte[] GetReportByPartnerId(FiltersReports filtersReports)
        {
            try
            {
                var orders = _repository.GetOrdersByPartner(filtersReports);

                var webReport = new WebReport();
                webReport.Report.Load("..\\Model\\Reports\\ReportsOrdersByPartner.frx");

                var table = ToDataTable(orders);

                DataSet dataSet = new DataSet();
                dataSet.Tables.Add(table);
                webReport.Report.RegisterData(table, "orders_orders");

                webReport.Report.SetParameterValue("PartnerName", orders.First().Legal_name);
                webReport.Report.SetParameterValue("Start_date", DateTime.Parse(filtersReports.Start_date));
                webReport.Report.SetParameterValue("End_date", DateTime.Parse(filtersReports.End_date).ToString("dd/MM/yyyy"));
                webReport.Report.Prepare();

                using MemoryStream memoryStream = new MemoryStream();

                webReport.Report.Export(new PDFSimpleExport(), memoryStream);
                memoryStream.Flush();

                byte[] arrayReport = memoryStream.ToArray();

                return arrayReport;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public DataTable ToDataTable<T>(List<T> list)
        {
            DataTable dataTable = new DataTable();
            PropertyInfo[] propertyInfo = typeof(T).GetProperties();

            foreach (PropertyInfo property in propertyInfo)
            {
                dataTable.Columns.Add(new DataColumn(property.Name, property.PropertyType));
            }

            foreach (T item in list)
            {
                DataRow row = dataTable.NewRow();
                foreach (PropertyInfo property in propertyInfo)
                {
                    row[property.Name] = property.GetValue(item, null);
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
