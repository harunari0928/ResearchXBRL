using System.Linq;
using System;
using System.Threading.Tasks;
using Moq;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus;
using Xunit;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.AnalysisMenus
{
    public sealed class CreateAnalysisMenusInteractorTests
    {
        [Fact]
        public async Task リポジトリからメニューを取得し返す()
        {
            // arrange
            var repository = new Mock<IAnalysisMenuRepository>();
            var expected = new AnalysisMenu
            {
                AccountItems = new AccountItem[]
                {
                    new AccountItem
                    {
                        Name = "売掛金"
                    },
                    new AccountItem
                    {
                        Name = "売上高"
                    }
                },
                Corporations = new Corporation[]
                {
                    new Corporation
                    {
                        Name = "オリエンタ"
                    },
                    new Corporation
                    {
                        Name = "ル"
                    },
                    new Corporation
                    {
                        Name = "酵母"
                    }
                }
            };
            repository
                .Setup(x => x.Get())
                .ReturnsAsync(expected);
            var interactor = new CreateAnalysisMenusInteractor(repository.Object);

            // act
            var acutal = await interactor.Handle();

            // assert
            foreach (var (e, a) in expected.AccountItems.Zip(acutal.AccountItems))
            {
                Assert.Equal(e.Name, a);
            }
            foreach (var (e, a) in expected.Corporations.Zip(acutal.Corporations))
            {
                Assert.Equal(e.Name, a);
            }
        }
    }
}
