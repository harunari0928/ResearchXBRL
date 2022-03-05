using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialAnalysisAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;
using Xunit;
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Tests.Presentation.FinancialAnalysisAPI.Controllers;

public class PerformanceIndicatorsControllerTests
{
    private readonly Mock<ILogger<PerformanceIndicatorsController>> loggerMock;
    private readonly Mock<IGetPerformanceIndicatorsUsecase> usecaseMock;

    public PerformanceIndicatorsControllerTests()
    {
        loggerMock = new();
        usecaseMock = new();
    }

    public sealed class GetPerformanceIndicators : PerformanceIndicatorsControllerTests
    {
        [Fact]
        public async Task 企業の業績指標が返る()
        {
            // arrange
            var corporationId = "tekitou";
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
            usecaseMock.Setup(x => x.Handle(corporationId))
                .ReturnsAsync(new PerformanceIndicatorViewModel(expected));
            var controller = new PerformanceIndicatorsController(loggerMock.Object, usecaseMock.Object);

            // act
            var actual = (await controller.GetPerformanceIndicators(corporationId)).Value;

            if (actual is null)
            {
                throw new Exception("結果を返しませんでした");
            }

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

        [Fact]
        public async Task UsecaseからArgumentExceptionが出力されたときBadRequestを返す()
        {
            // arrange
            usecaseMock
                .Setup(x => x.Handle(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException());
            var controller = new PerformanceIndicatorsController(
                loggerMock.Object,
                usecaseMock.Object);

            // act
            var response = await controller.GetPerformanceIndicators("");

            // assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }
    }
}
