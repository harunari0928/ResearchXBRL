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
                    Values = new List<AccountValue>
                    {
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
                            {
                                From = new DateTime(2019, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2019, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
                            {
                                From = new DateTime(2022, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2022, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
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
                    .ReturnsAsync(expected.Corporation);
                analysisResultRepository
                    .Setup(x => x.GetNonConsolidateResult(
                        input.CorporationId,
                        input.AccountItemName))
                    .ReturnsAsync(expected);
                analysisResultRepository
                    .Setup(x => x.GetConsolidateResult(
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
                foreach (var (e, a) in expected.Values.Zip(acutal.Values))
                {
                    Assert.Equal(e.Amount, a.Amount);
                    Assert.Equal(e.FinalAccountsPeriod is DurationPeriod, a.FinalAccountsPeriod is DurationPeriodViewModel);
                    Assert.Equal(e.FinalAccountsPeriod is InstantPeriod, a.FinalAccountsPeriod is InstantPeriodViewModel);
                    if (e.FinalAccountsPeriod is DurationPeriod durationPeriod
                    && a.FinalAccountsPeriod is DurationPeriodViewModel durationPeriodViewModel)
                    {
                        Assert.Equal(durationPeriod.From, durationPeriodViewModel.From);
                        Assert.Equal(durationPeriod.To, durationPeriodViewModel.To);
                    }
                    else if (e.FinalAccountsPeriod is DurationPeriod instantPeriod
                    && a.FinalAccountsPeriod is DurationPeriodViewModel instantPeriodViewModel)
                    {
                        Assert.Equal(instantPeriod.From, instantPeriodViewModel.From);
                    }
                }

            }

            [Fact]
            public async Task 企業が非連結の場合単体財務諸表の分析結果を取得する()
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
                        IsLinking = false // 非連結
                    },
                    Values = new List<AccountValue>
                    {
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
                            {
                                From = new DateTime(2019, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2019, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
                            {
                                From = new DateTime(2022, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2022, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
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
                    .ReturnsAsync(expected.Corporation);
                analysisResultRepository
                    .Setup(x => x.GetNonConsolidateResult(
                        input.CorporationId,
                        input.AccountItemName))
                    .ReturnsAsync(expected);
                analysisResultRepository
                    .Setup(x => x.GetConsolidateResult(
                        input.CorporationId,
                        input.AccountItemName))
                    .ReturnsAsync(expected);
                var interactor = CreateInteractor();

                // act
                var acutal = await interactor.Handle(input);

                // assert
                analysisResultRepository
                    .Verify(x => x.GetNonConsolidateResult(It.IsAny<string>(), It.IsAny<string>()),
                        Times.Once);
                analysisResultRepository
                    .Verify(x => x.GetConsolidateResult(It.IsAny<string>(), It.IsAny<string>()),
                        Times.Never);
            }

            [Fact]
            public async Task 企業が連結の場合連結財務諸表の分析結果を取得する()
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
                        IsLinking = true // 連結
                    },
                    Values = new List<AccountValue>
                    {
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
                            {
                                From = new DateTime(2019, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2019, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
                            {
                                From = new DateTime(2022, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2022, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
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
                    .ReturnsAsync(expected.Corporation);
                analysisResultRepository
                    .Setup(x => x.GetNonConsolidateResult(
                        input.CorporationId,
                        input.AccountItemName))
                    .ReturnsAsync(expected);
                analysisResultRepository
                    .Setup(x => x.GetConsolidateResult(
                        input.CorporationId,
                        input.AccountItemName))
                    .ReturnsAsync(expected);
                var interactor = CreateInteractor();

                // act
                var acutal = await interactor.Handle(input);

                // assert
                analysisResultRepository
                    .Verify(x => x.GetNonConsolidateResult(It.IsAny<string>(), It.IsAny<string>()),
                        Times.Never);
                analysisResultRepository
                    .Verify(x => x.GetConsolidateResult(It.IsAny<string>(), It.IsAny<string>()),
                        Times.Once);
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
                    Values = new List<AccountValue>
                    {
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
                            {
                                From = new DateTime(2019, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2019, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
                            {
                                From = new DateTime(2022, 12, 11, 10, 9, 10, 11),
                                To = new DateTime(2022, 12, 31, 10, 9, 10, 11),
                            }
                        },
                        new AccountValue
                        {
                            FinalAccountsPeriod = new DurationPeriod
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
                    .Setup(x => x.GetNonConsolidateResult(
                        input.CorporationId,
                        input.AccountItemName))
                    .ReturnsAsync(expected);
                analysisResultRepository
                    .Setup(x => x.GetConsolidateResult(
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
