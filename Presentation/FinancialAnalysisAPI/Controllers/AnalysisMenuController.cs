using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus;

namespace FinancialAnalysisAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnalysisMenuController : ControllerBase
    {
        private readonly ILogger<AnalysisMenuController> logger;
        private readonly ICreateAnalysisMenusUsecase usecase;

        public AnalysisMenuController(
            ILogger<AnalysisMenuController> logger,
            ICreateAnalysisMenusUsecase usecase)
        {
            this.logger = logger;
            this.usecase = usecase;
        }

        [HttpGet]
        public async Task<AnalysisMenuViewModel> Get()
        {
            return await usecase.Handle();
        }
    }
}
