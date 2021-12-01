using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.TimeSeriesAnalysis;

namespace FinancialAnalysisAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TimeSeriesAnalysisContrller : ControllerBase
    {
        private readonly ILogger<TimeSeriesAnalysisContrller> logger;
        private readonly IPerformTimeSeriesAnalysisUseCase usecase;

        public TimeSeriesAnalysisContrller(
            ILogger<TimeSeriesAnalysisContrller> logger,
            IPerformTimeSeriesAnalysisUseCase usecase)
        {
            this.logger = logger;
            this.usecase = usecase;
        }

        [Route("timeSeriesAnalysisResult")]
        [HttpGet]
        public async Task<TimeSeriesAnalysisViewModel> GetTimeSeriesAnalysisResult(
            string corporationId,
             string accountItemName)
        {
            return await usecase.Handle(new AnalyticalMaterials
            {
                CorporationId = corporationId,
                AccountItemName = accountItemName
            });
        }
    }
}
