using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.AccountPeriods;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Units;
using Xunit;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class PerformTimeSeriesAnalysisInteractorTests
    {
        public sealed class HandleTests
        {
            private readonly Mock<ITimeSeriesAnalysisResultRepository> analysisResultRepository;
            private readonly Mock<ICorporationRepository> corporationRepository;

            public HandleTests()
            {
                analysisResultRepository = new();
                corporationRepository = new();
            }

            [Fact]
            public async Task 引数の企業IDと勘定項目に関する時系列分析結果を返す()
            {
                // arrange
                var expected = new TimeSeriesAnalysisResult
                {
                    AccountName = "会計項目名",
                    Unit = new NormalUnit
                    {
                        Name = "JPY",
                        Measure = "てきとう"
                    },
                    Corporation = new Corporation
                    {
                        Name = "変な会社",
                        CapitalAmount = 114514,
                        IsLinking = false
                    },
                    ConsolidatedValues = new List<AccountValue>
                    {
                        new AccountValue
                        {
                            FinancialAccountPeriod = new DurationPeriod
                            {
                                From = new DateTime(2019, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2019, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinancialAccountPeriod = new DurationPeriod
                            {
                                From = new DateTime(2022, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2022, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinancialAccountPeriod = new DurationPeriod
                            {
                                From = new DateTime(2021, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2021, 12, 31, 10, 9, 10, 11),
                            }
                        }
                    },
                    NonConsolidatedValues = new List<AccountValue>
                    {
                        new AccountValue
                        {
                            FinancialAccountPeriod = new InstantPeriod {
                                Instant = new DateTime(2016, 01, 3, 10, 19, 40)
                            }
                        },
                        new AccountValue
                        {
                            FinancialAccountPeriod = new DurationPeriod
                            {
                                From = new DateTime(2022, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2022, 12, 31, 10, 9, 10, 11),
                            }
                        }
                    }
                };
                var input = new AnalyticalMaterials
                {
                    CorporationId = "testCor",
                    AccountItemName = "test"
                };
                corporationRepository.Setup(x => x.Get(It.IsAny<string>()))
                    .ReturnsAsync(expected.Corporation);
                analysisResultRepository
                    .Setup(x => x.GetResult(
                        input.CorporationId,
                        input.AccountItemName))
                    .ReturnsAsync(expected);
                var interactor = CreateInteractor();

                // act
                var acutal = await interactor.Handle(input);

                // assert
                Assert.Equal(expected.AccountName, acutal.AccountName);
                Assert.Equal(expected.Unit.Name, acutal.Unit?.Name);
                Assert.Equal(expected.Corporation.CapitalAmount, acutal.Corporation.CapitalAmount);
                Assert.Equal(expected.Corporation.IsLinking, acutal.Corporation.IsLinking);
                Assert.Equal(expected.Corporation.Name, acutal.Corporation.Name);
                Assert.Equal(expected.Corporation.TypeOfIndustry, acutal.Corporation.TypeOfIndustry);
                AssertAccountValues(expected.ConsolidatedValues, acutal.ConsolidatedValues);
                AssertAccountValues(expected.NonConsolidatedValues, acutal.NonConsolidatedValues);
            }

            private static void AssertAccountValues(IReadOnlyList<AccountValue> expected, IReadOnlyList<AccountValueViewModel> acutal)
            {
                foreach (var (e, a) in expected.Zip(acutal))
                {
                    Assert.Equal(e.Amount, a.Amount);
                    if (e.FinancialAccountPeriod is DurationPeriod durationPeriod)
                    {
                        Assert.Equal(durationPeriod.From, a.FinancialAccountPeriod.From);
                        Assert.Equal(durationPeriod.To, a.FinancialAccountPeriod.To);
                    }
                    else if (e.FinancialAccountPeriod is InstantPeriod instantPeriod)
                    {
                        Assert.Equal(instantPeriod.Instant, a.FinancialAccountPeriod.Instant);
                    }
                }
            }

            [Fact]
            public async Task 引数で指定した企業が存在しなければArgumentExceptionを発生させる()
            {
                // arrange
                var expected = new TimeSeriesAnalysisResult
                {
                    AccountName = "会計項目名",
                    Unit = new NormalUnit
                    {
                        Name = "JPY",
                        Measure = "てきとう"
                    },
                    Corporation = new Corporation
                    {
                        Name = "変な会社",
                        CapitalAmount = 114514,
                        IsLinking = false
                    },
                    ConsolidatedValues = new List<AccountValue>
                    {
                        new AccountValue
                        {
                            FinancialAccountPeriod = new DurationPeriod
                            {
                                From = new DateTime(2019, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2019, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinancialAccountPeriod = new DurationPeriod
                            {
                                From = new DateTime(2022, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2022, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinancialAccountPeriod = new DurationPeriod
                            {
                                From = new DateTime(2021, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2021, 12, 31, 10, 9, 10, 11),
                            }
                        }
                    }
                };
                var input = new AnalyticalMaterials
                {
                    CorporationId = "testCor",
                    AccountItemName = "test"
                };
                corporationRepository.Setup(x => x.Get(It.IsAny<string>()))
                    .ReturnsAsync(null as Corporation);
                analysisResultRepository
                    .Setup(x => x.GetResult(
                        input.CorporationId,
                        input.AccountItemName))
                    .ReturnsAsync(expected);
                var interactor = CreateInteractor();

                // act & assert
                await Assert.ThrowsAsync<ArgumentException>(() => interactor.Handle(input));
            }
            private PerformTimeSeriesAnalysisInteractor CreateInteractor()
            {
                return new PerformTimeSeriesAnalysisInteractor(
                    analysisResultRepository.Object,
                    corporationRepository.Object);
            }
        }
    }
}
