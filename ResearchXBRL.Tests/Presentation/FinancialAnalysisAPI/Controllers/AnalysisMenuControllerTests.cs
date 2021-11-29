using System.Threading.Tasks;
using FinancialAnalysisAPI.Controllers;
using Microsoft.Extensions.Logging;
using Moq;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus;
using Xunit;

namespace ResearchXBRL.Tests.Presentation.FinancialAnalysisAPI.Controllers
{
    public sealed class AnalysisMenuControllerTests
    {
        public sealed class GetTests
        {
            private readonly Mock<ILogger<AnalysisMenuController>> logger = new();
            private readonly Mock<ICreateAnalysisMenusUsecase> usecase = new();

            [Fact]
            public async Task Usecase層から返されたViewModelをそのまま返す()
            {
                // arrange
                var expected = new AnalysisMenuViewModel
                {
                    AccountItems = new string[] { "未払い金", "買掛金", "売掛金" },
                    Corporations = new string[] { "t", "e", "s", "t" }
                };
                usecase
                    .Setup(x => x.Handle())
                    .ReturnsAsync(expected);
                var controller = new AnalysisMenuController(logger.Object, usecase.Object);

                // act
                var acutal = await controller.Get();

                // assert
                Assert.StrictEqual(expected, acutal);
            }
        }
    }
}
