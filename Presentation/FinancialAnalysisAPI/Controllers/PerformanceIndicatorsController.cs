using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.PerformanceIndicators;

namespace FinancialAnalysisAPI.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class PerformanceIndicatorsController
{
    private readonly ILogger<PerformanceIndicatorsController> logger;
    private readonly IGetPerformanceIndicatorsUsecase usecase;

    public PerformanceIndicatorsController(
        ILogger<PerformanceIndicatorsController> logger,
        IGetPerformanceIndicatorsUsecase usecase)
    {
        this.logger = logger;
        this.usecase = usecase;
    }

    [HttpGet]
    public async ValueTask<ActionResult<PerformanceIndicatorViewModel>> GetPerformanceIndicators(
            string corporationId)
    {
        try
        {
            return await usecase.Handle(corporationId);
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
