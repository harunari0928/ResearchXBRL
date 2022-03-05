using System.Linq;
using System.Threading.Tasks;
using System;
using Xunit;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.PerformanceIndicators;
using Moq;
using System.Collections.Generic;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;
using ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.PerformanceIndicators;

public sealed class GetPerformanceIndicatorsInteractorTests
{
    public sealed class HandleTests
    {
        private readonly Mock<IPerformanceIndicatorQueryService> queryServiceMock = new();
        private readonly Mock<ICorporationsQueryService> corporationQueryServiceMock = new();

        [Fact]
        public async Task 指定した企業IDの企業が存在しなければArgumentExceptionを発生させる()
        {
            // arrange
            var companyName = "tekitou";
            corporationQueryServiceMock
                .Setup(x => x.Exists(companyName))
                .ReturnsAsync(false);
            var interactor = new GetPerformanceIndicatorsInteractor(corporationQueryServiceMock.Object, queryServiceMock.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await interactor.Handle(companyName));
        }

        [Fact]
        public async Task 企業の業績指標が返る()
        {
            // arrange
            var companyName = "tekitou2";
            var expected = new PerformanceIndicator
            {
                Indicators = new List<Indicator>
                {
                    new Indicator
                    {
                        IndicatorType = IndicatorType.NetSales,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2017, 3, 1), 36 },
                            { new DateOnly(2018, 3, 1), 3 },
                            { new DateOnly(2019, 3, 1), 3 },
                        }
                    },
                    new Indicator
                    {
                        IndicatorType = IndicatorType.OperatingIncome,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2017, 6, 1), 38},
                            { new DateOnly(2018, 6, 1), 4 },
                            { new DateOnly(2019, 6, 1), 87 },
                        }
                    },
                    new Indicator
                    {
                        IndicatorType = IndicatorType.OrdinaryIncome,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2017, 10, 1), 213 },
                            { new DateOnly(2018, 10, 1), 11111 },
                            { new DateOnly(2019, 10, 1), 111144 },
                        }
                    },
                    new Indicator
                    {
                        IndicatorType = IndicatorType.ProfitLossAttributableToOwnersOfParent,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2017, 10, 1), 2414 },
                            { new DateOnly(2018, 10, 1), 23432 },
                            { new DateOnly(2019, 10, 1), 32423 },
                        }
                    },
                    new Indicator
                    {
                        IndicatorType = IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2018, 10, 1), 987 },
                            { new DateOnly(2019, 10, 1), 453 },
                            { new DateOnly(2020, 10, 1), 3432 },
                        }
                    },
                    new Indicator
                    {
                        IndicatorType = IndicatorType.DividendPaidPerShareSummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2018, 10, 1), 30 },
                            { new DateOnly(2019, 10, 1), 31 },
                            { new DateOnly(2020, 10, 1), 453 },
                        }
                    },
                }
            };
            queryServiceMock
                .Setup(x => x.Get(companyName))
                .ReturnsAsync(expected);

            corporationQueryServiceMock
                .Setup(x => x.Exists(companyName))
                .ReturnsAsync(true);
            var interactor = new GetPerformanceIndicatorsInteractor(corporationQueryServiceMock.Object, queryServiceMock.Object);

            // act
            var actual = await interactor.Handle(companyName);

            // assert
            Assert.Equal(expected.Indicators.Count, actual.Indicators.Count);
            foreach (var (expectedIndicators, actualIndicators) in expected.Indicators.Zip(actual.Indicators))
            {
                Assert.Equal((int)expectedIndicators.IndicatorType, (int)actualIndicators.IndicatorType);
                foreach (var (expectedValues, actualValues) in expectedIndicators.Values.Zip(actualIndicators.Values))
                {
                    Assert.Equal(expectedValues.Key.ToDateTime(TimeOnly.MinValue), actualValues.Key);
                    Assert.Equal(expectedValues.Value, actualValues.Value);
                }
            }
        }
    }
}
