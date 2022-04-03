using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.AnalysisMenus.AccountItems;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.AnalysisMenus.Corporations;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.CorporationMenus;

namespace FinancialAnalysisAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnalysisMenuController : ControllerBase
    {
        private readonly ILogger<AnalysisMenuController> logger;
        private readonly ISuggestAccountItemsUsecase suggestAccountItemUsecase;
        private readonly ISuggestCorporationsUsecase suggestCorporationUsecase;

        public AnalysisMenuController(
            ILogger<AnalysisMenuController> logger,
            ISuggestAccountItemsUsecase suggestAccountItemUsecase,
            ISuggestCorporationsUsecase suggestCorporationUsecase)
        {
            this.logger = logger;
            this.suggestAccountItemUsecase = suggestAccountItemUsecase;
            this.suggestCorporationUsecase = suggestCorporationUsecase;
        }

        [Route("suggest/accountItems")]
        [HttpGet]
        public async Task<IReadOnlyList<AccountItemViewModel>> SuggestAccountItems(string keyword)
        {
            return await suggestAccountItemUsecase
                .Handle(keyword);
        }

        [Route("suggest/corporations")]
        [HttpGet]
        public async Task<IReadOnlyList<CorporationViewModel>> SuggestCorporations(string keyword)
        {
            return await suggestCorporationUsecase
                .Handle(keyword);
        }
    }
}
