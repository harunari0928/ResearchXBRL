using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialAnalysisAPI.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using ResearchXBRL.Application.DTO.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Application.DTO.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using Xunit;

namespace ResearchXBRL.Tests.Presentation.FinancialAnalysisAPI.Controllers
{
    public class AnalysisMenuControllerTests
    {
        private readonly Mock<ILogger<AnalysisMenuController>> logger;
        private readonly Mock<ISuggestAccountItemsUsecase> suggestAccountItemUsecase;
        private readonly Mock<ISuggestCorporationsUsecase> suggestCorporationUsecase;

        public AnalysisMenuControllerTests()
        {
            logger = new();
            suggestAccountItemUsecase = new();
            suggestCorporationUsecase = new();
        }

        public sealed class SuggestAccountItemsTests : AnalysisMenuControllerTests
        {
            [Fact]
            public async Task サジェスト結果を返す()
            {
                // arrange
                var expected = new List<AccountItemViewModel>
                {
                    new AccountItemViewModel
                    {
                        Name = "test1"
                    },
                    new AccountItemViewModel
                    {
                        Name = "test2"
                    }
                };
                var keyword = "会計項目";
                suggestAccountItemUsecase
                    .Setup(x => x.Handle(keyword))
                    .ReturnsAsync(expected);
                var controller = CreateController();

                // act
                var acutal = await controller.SuggestAccountItems(keyword);

                // actual
                Assert.StrictEqual(expected, acutal);
            }
        }

        public sealed class SuggestCorporationsTests : AnalysisMenuControllerTests
        {
            [Fact]
            public async Task サジェスト結果を返す()
            {
                // arrange
                var expected = new List<CorporationViewModel>
                {
                    new CorporationViewModel
                    {
                        Name = "test1",
                        CorporationId = "tsetstst"
                    },
                    new CorporationViewModel
                    {
                        Name = "test1",
                        CorporationId = "fffff"
                    }
                };
                var keyword = "テスト企業";
                suggestCorporationUsecase
                    .Setup(x => x.Handle(keyword))
                    .ReturnsAsync(expected);
                var controller = CreateController();

                // act
                var acutal = await controller.SuggestCorporations(keyword);

                // actual
                Assert.StrictEqual(expected, acutal);
            }
        }

        private AnalysisMenuController CreateController()
        {
            return new AnalysisMenuController(
                        logger.Object,
                        suggestAccountItemUsecase.Object,
                        suggestCorporationUsecase.Object);
        }
    }
}
