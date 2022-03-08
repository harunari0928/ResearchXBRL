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
using ResearchXBRL.Application.ViewModel.FinancialAnalysis.PerformanceIndicators.Indicators;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.PerformanceIndicators;

public sealed class GetPerformanceIndicatorsInteractorTests
{
    public class HandleTests
    {
        private readonly Mock<IPerformanceIndicatorsQueryService> performanceIndicatorsQueryServiceMock = new();
        private readonly Mock<ICorporationsQueryService> corporationQueryServiceMock = new();
        private readonly Mock<ITimeseriesAccountValuesQueryService> timeseriesAccountValuesQueryServiceMock = new();

        [Fact]
        public async Task 指定した企業IDの企業が存在しなければArgumentExceptionを発生させる()
        {
            // arrange
            var companyName = "tekitou";
            corporationQueryServiceMock
                .Setup(x => x.Exists(companyName))
                .ReturnsAsync(false);
            var interactor = new GetPerformanceIndicatorsInteractor(
                corporationQueryServiceMock.Object,
                performanceIndicatorsQueryServiceMock.Object,
                timeseriesAccountValuesQueryServiceMock.Object);

            // act & assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await interactor.Handle(companyName));
        }

        [Fact]
        public async Task 企業の業績指標が返る()
        {
            // arrange
            var companyName = "tekitou2";
            var expected = GetSamplePerformanceIndicator();
            performanceIndicatorsQueryServiceMock
                .Setup(x => x.Get(companyName))
                .ReturnsAsync(expected);

            var profitOrLoss = new Dictionary<DateOnly, decimal>
            {
                { new DateOnly(2017, 10, 1), decimal.MaxValue },
                { new DateOnly(2018, 10, 1), decimal.MaxValue },
                { new DateOnly(2019, 10, 1), decimal.MaxValue },
                { new DateOnly(2020, 10, 1), decimal.MaxValue },
                { new DateOnly(2021, 10, 1), decimal.MaxValue },
            };
            timeseriesAccountValuesQueryServiceMock
                .Setup(x => x.Get(companyName, "当期純利益又は当期純損失（△）"))
                .ReturnsAsync(profitOrLoss);
            var capital = new Dictionary<DateOnly, decimal>
            {
                { new DateOnly(2017, 10, 1), decimal.MaxValue  },
                { new DateOnly(2018, 10, 1), decimal.MaxValue },
                { new DateOnly(2019, 10, 1), decimal.MaxValue },
                { new DateOnly(2020, 10, 1), decimal.MaxValue },
                { new DateOnly(2021, 10, 1), decimal.MaxValue },
            };
            timeseriesAccountValuesQueryServiceMock
                .Setup(x => x.Get(companyName, "資本金"))
                .ReturnsAsync(capital);

            corporationQueryServiceMock
                .Setup(x => x.Exists(companyName))
                .ReturnsAsync(true);
            var interactor = new GetPerformanceIndicatorsInteractor(
                corporationQueryServiceMock.Object,
                performanceIndicatorsQueryServiceMock.Object,
                timeseriesAccountValuesQueryServiceMock.Object);

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

        [Fact(DisplayName = "ROEを取得したがある年度の欠損がある場合,その部分についてのみ計算を行う")]
        public async Task Test7()
        {
            // arrange
            var companyName = "tekitou2";
            // 2018,2020年のみ欠落
            var indicators = GetSamplePerformanceIndicatorWithMissingROE();
            performanceIndicatorsQueryServiceMock
                .Setup(x => x.Get(companyName))
                .ReturnsAsync(indicators);
            var profitOrLoss = new Dictionary<DateOnly, decimal>
            {
                { new DateOnly(2017, 10, 1), decimal.MaxValue },
                { new DateOnly(2018, 10, 1), decimal.MaxValue },
                { new DateOnly(2019, 10, 1), decimal.MaxValue },
                { new DateOnly(2020, 10, 1), decimal.MaxValue },
                { new DateOnly(2021, 10, 1), decimal.MaxValue },
            };
            timeseriesAccountValuesQueryServiceMock
                .Setup(x => x.Get(companyName, "当期純利益又は当期純損失（△）"))
                .ReturnsAsync(profitOrLoss);
            var capital = new Dictionary<DateOnly, decimal>
            {
                { new DateOnly(2017, 10, 1), decimal.MaxValue  },
                { new DateOnly(2018, 10, 1), decimal.MaxValue },
                { new DateOnly(2019, 10, 1), decimal.MaxValue },
                { new DateOnly(2020, 10, 1), decimal.MaxValue },
                { new DateOnly(2021, 10, 1), decimal.MaxValue },
            };
            timeseriesAccountValuesQueryServiceMock
                .Setup(x => x.Get(companyName, "資本金"))
                .ReturnsAsync(capital);
            var expectedROEValues = profitOrLoss
                .Zip(capital, (p, c) => new KeyValuePair<DateOnly, decimal>(p.Key, p.Value / c.Value))
                .OrderBy(x => x.Key);

            corporationQueryServiceMock
                .Setup(x => x.Exists(companyName))
                .ReturnsAsync(true);
            var interactor = new GetPerformanceIndicatorsInteractor(
                corporationQueryServiceMock.Object,
                performanceIndicatorsQueryServiceMock.Object,
                timeseriesAccountValuesQueryServiceMock.Object);

            // act
            var viewModel = await interactor.Handle(companyName);
            var actual = viewModel.Indicators
                .Single(x => x.IndicatorType == IndicatorTypeViewModel.RateOfReturnOnEquitySummaryOfBusinessResults)
                .Values;

            // assert
            // 2018,2020年のみ計算を行う
            Assert.Equal(profitOrLoss[new DateOnly(2018, 10, 1)] / capital[new DateOnly(2018, 10, 1)], actual[new DateTime(2018, 10, 1)]);
            Assert.Equal(profitOrLoss[new DateOnly(2020, 10, 1)] / capital[new DateOnly(2020, 10, 1)], actual[new DateTime(2020, 10, 1)]);
            //  それ以外の年はQueryServiceから取得した値をそのまま使う
            var expectedROEsFromQueryService = indicators.Indicators
                .Single(x => x.IndicatorType == IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults)
                .Values;
            Assert.Equal(expectedROEsFromQueryService[new DateOnly(2017, 10, 1)], actual[new DateTime(2017, 10, 1)]);
            Assert.Equal(expectedROEsFromQueryService[new DateOnly(2019, 10, 1)], actual[new DateTime(2019, 10, 1)]);
            Assert.Equal(expectedROEsFromQueryService[new DateOnly(2021, 10, 1)], actual[new DateTime(2021, 10, 1)]);
        }

        public sealed class ROEの値を取得できなかった場合
        {
            private readonly Mock<IPerformanceIndicatorsQueryService> performanceIndicatorsQueryServiceMock = new();
            private readonly Mock<ICorporationsQueryService> corporationQueryServiceMock = new();
            private readonly Mock<ITimeseriesAccountValuesQueryService> timeseriesAccountValuesQueryServiceMock = new();

            [Fact(DisplayName = "純利益と自己資本を元に計算する")]
            public async Task Test3()
            {
                // arrange
                var companyName = "tekitou2";
                var indicators = GetSamplePerformanceIndicatorWithEmptyROE();
                performanceIndicatorsQueryServiceMock
                    .Setup(x => x.Get(companyName))
                    .ReturnsAsync(indicators);
                var profitOrLoss = new Dictionary<DateOnly, decimal>
                {
                    { new DateOnly(2017, 2, 1), 10000 },
                    { new DateOnly(2018, 2, 1), 11000 },
                    { new DateOnly(2019, 2, 1), 12000 },
                    { new DateOnly(2020, 2, 1), 13000 },
                    { new DateOnly(2021, 2, 1), 14000 },
                };
                timeseriesAccountValuesQueryServiceMock
                    .Setup(x => x.Get(companyName, "当期純利益又は当期純損失（△）"))
                    .ReturnsAsync(profitOrLoss);
                var capital = new Dictionary<DateOnly, decimal>
                {
                    { new DateOnly(2017, 2, 1), 1000 },
                    { new DateOnly(2018, 2, 1), 1100 },
                    { new DateOnly(2019, 2, 1), 1200 },
                    { new DateOnly(2020, 2, 1), 1300 },
                    { new DateOnly(2021, 2, 1), 1400 },
                };
                timeseriesAccountValuesQueryServiceMock
                    .Setup(x => x.Get(companyName, "資本金"))
                    .ReturnsAsync(capital);
                var expectedROEValues = profitOrLoss
                    .Zip(capital, (p, c) => new KeyValuePair<DateOnly, decimal>(p.Key, p.Value / c.Value))
                    .OrderBy(x => x.Key);

                corporationQueryServiceMock
                    .Setup(x => x.Exists(companyName))
                    .ReturnsAsync(true);
                var interactor = new GetPerformanceIndicatorsInteractor(
                    corporationQueryServiceMock.Object,
                    performanceIndicatorsQueryServiceMock.Object,
                    timeseriesAccountValuesQueryServiceMock.Object);

                // act
                var viewModel = await interactor.Handle(companyName);
                var actual = viewModel.Indicators
                    .Single(x => x.IndicatorType == IndicatorTypeViewModel.RateOfReturnOnEquitySummaryOfBusinessResults)
                    .Values.OrderBy(x => x.Key);

                // assert
                Assert.Equal(expectedROEValues.Count(), actual.Count());
                foreach (var (expected, acutal) in expectedROEValues.Zip(actual))
                {
                    Assert.Equal(expected.Key.ToDateTime(TimeOnly.MinValue), acutal.Key);
                    Assert.Equal(expected.Value, acutal.Value);
                }
            }

            [Fact(DisplayName = "資本金のある年度の値が存在しない場合、その年のROEは計算しない")]
            public async Task Test5()
            {
                // arrange
                var companyName = "tekitou2";
                var indicators = GetSamplePerformanceIndicatorWithEmptyROE();
                performanceIndicatorsQueryServiceMock
                    .Setup(x => x.Get(companyName))
                    .ReturnsAsync(indicators);
                var profitOrLoss = new Dictionary<DateOnly, decimal>
                {
                    { new DateOnly(2017, 2, 1), 10000 },
                    { new DateOnly(2018, 2, 1), 11000 },
                    { new DateOnly(2019, 2, 1), 12000 },
                    { new DateOnly(2020, 2, 1), 13000 },
                    { new DateOnly(2021, 2, 1), 14000 },
                };
                timeseriesAccountValuesQueryServiceMock
                    .Setup(x => x.Get(companyName, "当期純利益又は当期純損失（△）"))
                    .ReturnsAsync(profitOrLoss);
                var capital = new Dictionary<DateOnly, decimal>
                {
                    { new DateOnly(2017, 2, 1), 1000 },
                    { new DateOnly(2018, 2, 1), 1100 },
                    // 2019年が存在しない
                    { new DateOnly(2020, 2, 1), 1300 },
                    { new DateOnly(2021, 2, 1), 1400 },
                };
                timeseriesAccountValuesQueryServiceMock
                    .Setup(x => x.Get(companyName, "資本金"))
                    .ReturnsAsync(capital);
                var expectedROEValues = profitOrLoss.Where(x => x.Key != new DateOnly(2019, 2, 1))
                    .Zip(capital, (p, c) => new KeyValuePair<DateOnly, decimal>(p.Key, p.Value / c.Value));

                corporationQueryServiceMock
                    .Setup(x => x.Exists(companyName))
                    .ReturnsAsync(true);
                var interactor = new GetPerformanceIndicatorsInteractor(
                    corporationQueryServiceMock.Object,
                    performanceIndicatorsQueryServiceMock.Object,
                    timeseriesAccountValuesQueryServiceMock.Object);

                // act
                var viewModel = await interactor.Handle(companyName);
                var actual = viewModel.Indicators
                    .Single(x => x.IndicatorType == IndicatorTypeViewModel.RateOfReturnOnEquitySummaryOfBusinessResults)
                    .Values;

                // assert
                Assert.False(actual.ContainsKey(new DateTime(2019, 2, 1)));
            }

            [Fact(DisplayName = "当期利益のある年度の値が存在しない場合、その年のROEは計算しない")]
            public async Task Test6()
            {
                // arrange
                var companyName = "tekitou2";
                var indicators = GetSamplePerformanceIndicatorWithEmptyROE();
                performanceIndicatorsQueryServiceMock
                    .Setup(x => x.Get(companyName))
                    .ReturnsAsync(indicators);
                var profitOrLoss = new Dictionary<DateOnly, decimal>
                {
                    { new DateOnly(2017, 2, 1), 10000 },
                    { new DateOnly(2018, 2, 1), 11000 },
                    // 2019年が存在しない
                    { new DateOnly(2020, 2, 1), 13000 },
                    { new DateOnly(2021, 2, 1), 14000 },
                };
                timeseriesAccountValuesQueryServiceMock
                    .Setup(x => x.Get(companyName, "当期純利益又は当期純損失（△）"))
                    .ReturnsAsync(profitOrLoss);
                var capital = new Dictionary<DateOnly, decimal>
                {
                    { new DateOnly(2017, 2, 1), 1000 },
                    { new DateOnly(2018, 2, 1), 1100 },
                    { new DateOnly(2019, 2, 1), 1200 },
                    { new DateOnly(2020, 2, 1), 1300 },
                    { new DateOnly(2021, 2, 1), 1400 },
                };
                timeseriesAccountValuesQueryServiceMock
                    .Setup(x => x.Get(companyName, "資本金"))
                    .ReturnsAsync(capital);
                var expectedROEValues = profitOrLoss.Where(x => x.Key != new DateOnly(2019, 2, 1))
                    .Zip(capital, (p, c) => new KeyValuePair<DateOnly, decimal>(p.Key, p.Value / c.Value));

                corporationQueryServiceMock
                    .Setup(x => x.Exists(companyName))
                    .ReturnsAsync(true);
                var interactor = new GetPerformanceIndicatorsInteractor(
                    corporationQueryServiceMock.Object,
                    performanceIndicatorsQueryServiceMock.Object,
                    timeseriesAccountValuesQueryServiceMock.Object);

                // act
                var viewModel = await interactor.Handle(companyName);
                var actual = viewModel.Indicators
                    .Single(x => x.IndicatorType == IndicatorTypeViewModel.RateOfReturnOnEquitySummaryOfBusinessResults)
                    .Values;

                // assert
                Assert.False(actual.ContainsKey(new DateTime(2019, 2, 1)));
            }
        }

        [Fact(DisplayName = "ROEを取得しない場合,計算も行わない")]
        public async Task Test4()
        {
            // arrange
            var companyName = "tekitou2";
            var indicators = GetSamplePerformanceIndicatorWithoutROE();
            performanceIndicatorsQueryServiceMock
                .Setup(x => x.Get(companyName))
                .ReturnsAsync(indicators);
            var profitOrLoss = new Dictionary<DateOnly, decimal>
            {
                { new DateOnly(2017, 2, 1), 10000 },
                { new DateOnly(2018, 2, 1), 11000 },
                { new DateOnly(2019, 2, 1), 12000 },
                { new DateOnly(2020, 2, 1), 13000 },
                { new DateOnly(2021, 2, 1), 14000 },
            };
            timeseriesAccountValuesQueryServiceMock
                .Setup(x => x.Get(companyName, "当期純利益又は当期純損失（△）"))
                .ReturnsAsync(profitOrLoss);
            var capital = new Dictionary<DateOnly, decimal>
            {
                { new DateOnly(2017, 2, 1), 1000 },
                { new DateOnly(2018, 2, 1), 1100 },
                { new DateOnly(2019, 2, 1), 1200 },
                { new DateOnly(2020, 2, 1), 1300 },
                { new DateOnly(2021, 2, 1), 1400 },
            };
            timeseriesAccountValuesQueryServiceMock
                .Setup(x => x.Get(companyName, "資本金"))
                .ReturnsAsync(capital);
            var expectedROEValues = profitOrLoss.Zip(capital, (p, c) => new KeyValuePair<DateOnly, decimal>(p.Key, p.Value / c.Value));

            corporationQueryServiceMock
                .Setup(x => x.Exists(companyName))
                .ReturnsAsync(true);
            var interactor = new GetPerformanceIndicatorsInteractor(
                corporationQueryServiceMock.Object,
                performanceIndicatorsQueryServiceMock.Object,
                timeseriesAccountValuesQueryServiceMock.Object);

            // act
            var viewModel = await interactor.Handle(companyName);
            var actual = viewModel.Indicators
                .FirstOrDefault(x => x.IndicatorType == IndicatorTypeViewModel.RateOfReturnOnEquitySummaryOfBusinessResults);

            // assert
            Assert.Null(actual);
        }

        private static PerformanceIndicator GetSamplePerformanceIndicator()
        {
            return new PerformanceIndicator
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
                        IndicatorType = IndicatorType.DividendPaidPerShareSummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2018, 10, 1), 30 },
                            { new DateOnly(2019, 10, 1), 31 },
                            { new DateOnly(2020, 10, 1), 453 },
                        }
                    },
                    new Indicator
                    {
                        IndicatorType = IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2018, 10, 1), 30 },
                            { new DateOnly(2019, 10, 1), 31 },
                            { new DateOnly(2020, 10, 1), 453 },
                        }
                    },
                }
            };
        }
        private static PerformanceIndicator GetSamplePerformanceIndicatorWithEmptyROE()
        {
            return new PerformanceIndicator
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
                        IndicatorType = IndicatorType.DividendPaidPerShareSummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2018, 10, 1), 30 },
                            { new DateOnly(2019, 10, 1), 31 },
                            { new DateOnly(2020, 10, 1), 453 },
                        }
                    },
                    new Indicator
                    {
                        IndicatorType = IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>()
                    },
                }
            };
        }
        private static PerformanceIndicator GetSamplePerformanceIndicatorWithMissingROE()
        {
            return new PerformanceIndicator
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
                        IndicatorType = IndicatorType.DividendPaidPerShareSummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2018, 10, 1), 30 },
                            { new DateOnly(2019, 10, 1), 31 },
                            { new DateOnly(2020, 10, 1), 453 },
                        }
                    },
                    new Indicator
                    {
                        IndicatorType = IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            // 2018と2020年が欠落
                            { new DateOnly(2017, 10, 1), 30 },
                            { new DateOnly(2019, 10, 1), 31 },
                            { new DateOnly(2021, 10, 1), 453 },
                        }
                    },
                }
            };
        }
        private static PerformanceIndicator GetSamplePerformanceIndicatorWithoutROE()
        {
            return new PerformanceIndicator
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
                        IndicatorType = IndicatorType.DividendPaidPerShareSummaryOfBusinessResults,
                        Values = new Dictionary<DateOnly, decimal>
                        {
                            { new DateOnly(2018, 10, 1), 30 },
                            { new DateOnly(2019, 10, 1), 31 },
                            { new DateOnly(2020, 10, 1), 453 },
                        }
                    }
                }
            };
        }
    }
}
