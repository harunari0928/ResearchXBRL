using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.TimeSeriesAnalysis;

namespace FinancialAnalysisAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TimeSeriesAnalysisController : ControllerBase
    {
        private readonly ILogger<TimeSeriesAnalysisController> logger;
        private readonly IPerformTimeSeriesAnalysisUseCase usecase;

        public TimeSeriesAnalysisController(
            ILogger<TimeSeriesAnalysisController> logger,
            IPerformTimeSeriesAnalysisUseCase usecase)
        {
            this.logger = logger;
            this.usecase = usecase;
        }

        [Route("result")]
        [HttpGet]
        public async Task<ActionResult<TimeSeriesAnalysisViewModel>> GetTimeSeriesAnalysisResult(
            string corporationId,
             string accountItemName)
        {
            try
            {
                return await usecase.Handle(new AnalyticalMaterials
                {
                    CorporationId = corporationId,
                    AccountItemName = accountItemName
                });
            }
            catch (ArgumentException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
