using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.PerformanceIndicators;

namespace FinancialAnalysisAPI.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class PerformanceIndicatorsController
{
    private readonly ILogger<PerformanceIndicatorsController> logger;
    private readonly GetPerformanceIndicatorsInteractor usecase;

    public PerformanceIndicatorsController(
        ILogger<PerformanceIndicatorsController> logger,
        GetPerformanceIndicatorsInteractor usecase)
    {
        this.logger = logger;
        this.usecase = usecase;
    }

    [HttpGet]
    public async ValueTask<ActionResult<PerformanceIndicatorsViewModel>> GetPerformanceIndicators(
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
