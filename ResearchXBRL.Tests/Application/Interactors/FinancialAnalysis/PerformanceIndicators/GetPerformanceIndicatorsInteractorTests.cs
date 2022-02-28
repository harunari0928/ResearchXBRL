using System.Threading.Tasks;
using System;
using ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators;
using Xunit;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators.Corporations;
using Moq;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.PerformanceIndicators
{
    public sealed class GetPerformanceIndicatorsInteractorTests
    {
        public sealed class HandleTests
        {
            private readonly Mock<IPerformanceIndicatorsRepository> repositoryMock = new();
            private readonly Mock<ICorporationsRepository> corporationRepositoryMock = new();

            [Fact]
            public async Task 指定した企業IDの企業が存在しなければArgumentExceptionを発生させる()
            {
                // arrange
                corporationRepositoryMock
                    .Setup(x => x.Exists(It.IsAny<string>()))
                    .ReturnsAsync(false);
                var interactor = new GetPerformanceIndicatorsInteractor(repositoryMock.Object, corporationRepositoryMock.Object);

                // act & assert
                await Assert.ThrowsAsync<ArgumentException>(async () => await interactor.Handle("tekitou"));
            }
        }
    }
}
