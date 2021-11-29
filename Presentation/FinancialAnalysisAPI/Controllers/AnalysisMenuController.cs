using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResearchXBRL.Application.DTO;

namespace FinancialAnalysisAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnalysisMenuController : ControllerBase
    {
        private readonly ILogger<AnalysisMenuController> _logger;

        public AnalysisMenuController(ILogger<AnalysisMenuController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IAsyncEnumerable<AnalysisMenuViewModel> Get()
        {
            throw new NotImplementedException();
        }
    }
}
