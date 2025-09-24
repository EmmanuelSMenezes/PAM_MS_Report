using Application.Service;
using Application.Service.Interfaces;
using Domain.Model;
using FastReport;
using FastReport.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("report")]
    [ApiController]
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _service;
        private readonly ILogger _logger;
        public ReportController(IReportService service, ILogger logger) {
            _service = service;
            _logger = logger;
        }

        // <summary>
        /// Endpoint responsável por criar um novo relatório.
        /// </summary>
        /// <returns>Valida os dados passados e retorna os dados cadastrado.</returns>

        [HttpPost("create")]
        [ProducesResponseType(typeof(Response<Reports>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<Reports>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<Reports>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<Reports>> CreateReport([FromBody] ReportRequest createReportRequest)
        {
            try
            {
                var token = Request.Headers["Authorization"];
                var response = _service.CreateReport(createReportRequest, token);

                return StatusCode(StatusCodes.Status201Created, new Response<Reports>() { Status = StatusCodes.Status201Created, Message = "Relatório criado com sucesso!", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception when creating report!");
                switch (ex.Message)
                {
                    case "errorDecodingToken":
                        return StatusCode(StatusCodes.Status400BadRequest, new Response<Reports>() { Status = StatusCodes.Status400BadRequest, Message = $"Não foi possível criar relatório. Erro no processo de decodificação de token!", Data = null, Success = false, Error = ex.Message });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<Reports>() { Status = StatusCodes.Status500InternalServerError, Message = $"Internal server error! Exception Detail: {ex.Message}", Data = null, Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por listar todos os relatórios.
        /// </summary>
        /// <returns>Retorna lista com todos relatórios cadastrados.</returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(Response<ListReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ListReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ListReportResponse>), StatusCodes.Status500InternalServerError)]

        public ActionResult<Response<ListReportResponse>> GetAllReports(int? page, int? itensPerPage)
        {
            try
            {
                var filterPagination = new FilterPagination
                {
                    Page = page ?? 1,
                    ItensPerPage = itensPerPage ?? 5
                };
                var response = _service.GetReports(filterPagination);
                return StatusCode(StatusCodes.Status200OK, new Response<ListReportResponse>() { Status = 200, Message = $"Relatórios listados com sucesso.", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while listing reports!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ListReportResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por trazer um relatório especifico
        /// </summary>
        /// <returns>Retorna relatório por id</returns>
        [HttpGet("{report_id}")]
        [ProducesResponseType(typeof(Response<ReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<ReportResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<ReportResponse>), StatusCodes.Status500InternalServerError)]

        public ActionResult<Response<ReportResponse>> GetReportByReportId(Guid report_id)
        {
            try
            {
                var response = _service.GetReportByReportId(report_id);

                return StatusCode(StatusCodes.Status201Created, new Response<ReportResponse>() { Status = StatusCodes.Status201Created, Message = "Relatório retornado com sucesso!", Data = response, Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception when creating report!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ReportResponse>() { Status = StatusCodes.Status500InternalServerError, Message = $"Internal server error! Exception Detail: {ex.Message}", Data = null, Success = false, Error = ex.Message });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por atualizar o relatório
        /// </summary>
        /// <returns>Retorna o objeto que representa o relatório em caso de sucesso</returns>
        [HttpPut("update")]
        [ProducesResponseType(typeof(Response<Reports>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<Reports>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<Reports>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<Reports>> UpdateReport([FromBody] Reports report)
        {
            if (report.Report_id == Guid.Empty)
                return BadRequest(new Response<Reports>() { Status = 400, Message = "Report_id não informado", Success = false });

            try
            {
                var token = Request.Headers["Authorization"];
                var response = _service.UpdateReport(report, token);

                if (response != null)
                {
                    _logger.Information($"Relatório atualizado com sucesso!");
                    return StatusCode(StatusCodes.Status200OK, new Response<Reports>() { Status = 200, Data = response, Message = $"Relatório atualizado com sucesso", Success = true });
                }
                else
                {
                    _logger.Warning($"Não foi possível atualizar o relatório.");
                    return BadRequest(new Response<Reports>() { Status = 400, Message = $"Não foi possível atualizar o relatório.", Success = false });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while updateing shipping company!");
                switch (ex.Message)
                {
                    case "reportNotCreated":
                        return StatusCode(StatusCodes.Status204NoContent, new Response<Reports>() { Status = 304, Message = $"Não foi possível localizar o relatório na base de dados.", Success = false });
                    case "errorWhileUpdateReport":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<Reports>() { Status = 304, Message = $"Não foi possível atualizar o relatório na base de dados.", Success = false });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<Reports>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false });
                }
            }
        }

        /// <summary>
        /// Endpoint responsável por excluir um relatório
        /// </summary>
        /// <returns>Valida os dados passados para deleção do relatório</returns>      
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status500InternalServerError)]
        public ActionResult<Response<bool>> DeleteReport(List<Guid> report_id)
        {
            try
            {
                var response = _service.DeleteReportByReportId(report_id);
                return StatusCode(StatusCodes.Status200OK, new Response<ReportResponse>() { Status = 200, Message = $"Relatório excluído com sucesso.", Success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception while deleting shipping company!");
                switch (ex.Message)
                {
                    case "errorWhileDeleteReport":
                        return StatusCode(StatusCodes.Status304NotModified, new Response<ReportResponse>() { Status = 304, Message = $"Não foi possível deletar o relatório. Erro no processo de deleção de relatório na base de dados.", Success = false });
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<ReportResponse>() { Status = 500, Message = $"Internal server error! Exception Detail: {ex.Message}", Success = false });
                }
            }
        }
        /// <summary>
        /// Endpoint responsável por criar relátorio apartir de um parceiro
        /// </summary>
        /// <returns>Retorna relatório do parceiro</returns>
        [HttpGet("{partner_id}")]
        [ProducesResponseType(typeof(Response<FileContentResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<FileContentResult>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<FileContentResult>), StatusCodes.Status500InternalServerError)]

        public ActionResult<Response<FileContentResult>> GetReportByPartnerId(Guid partner_id, [Required(ErrorMessage = "Data inicial é obrigatório")] DateTime? start_date, [Required(ErrorMessage = "Data Final é obrigatório")] DateTime? end_date)
        {
            try
            {
               
                FiltersReports filtersReports = new FiltersReports()
                {
                    Partner_id = partner_id,
                    Start_date = start_date != null ? start_date.Value.ToString("yyyy-MM-dd 00:00:00") : DateTime.UtcNow.AddDays(-31).ToString("yyyy-MM-dd 00:00:00"),
                    End_date = end_date != null ? end_date.Value.ToString("yyyy-MM-dd 23:59:59") : DateTime.UtcNow.ToString("yyyy-MM-dd 00:00:00"),
                };

                bool days = (DateTime.Parse(filtersReports.End_date) - DateTime.Parse(filtersReports.Start_date)).Days > 31 ? false : true;

                if (!days) return StatusCode(StatusCodes.Status400BadRequest, new Response<FileContentResult>() { Status = StatusCodes.Status400BadRequest, Message = "Informe no máximo 31 dias para gerar o relatório", Data = null, Success = false });

                var response = _service.GetReportByPartnerId(filtersReports);

                return File(response, "application/zip", $"OrdersByPartner.pdf");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception when creating report!");
                switch (ex.Message)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response<FileContentResult>() { Status = StatusCodes.Status500InternalServerError, Message = $"Internal server error! Exception Detail: {ex.Message}", Data = null, Success = false, Error = ex.Message });
                }
            }
        }
    } 
}

