using System;
using System.Collections.Generic;
using System.Linq;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.AccountPeriods;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Units;

namespace ResearchXBRL.Application.DTO.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class TimeSeriesAnalysisViewModel
    {
        public string AccountName { get; init; } = "";
        public UnitViewModel? Unit { get; init; } = null;
        public CorporationViewModel Corporation { get; init; } = new CorporationViewModel();
        public IReadOnlyList<AccountValueViewModel> Values { get; init; } = new AccountValueViewModel[0];

        public TimeSeriesAnalysisViewModel() { }
        public TimeSeriesAnalysisViewModel(TimeSeriesAnalysisResult analysis)
        {
            AccountName = analysis.AccountName;
            Unit = MapToViewModel(analysis.Unit);
            Corporation = MapToViewModel(analysis);
            Values = MapToViewModel(analysis.Values);
        }

        private static UnitViewModel? MapToViewModel(IUnit? unit) => unit switch
        {
            NormalUnit normalUnit => new UnitViewModel
            {
                Name = normalUnit.Name,
                Measure = normalUnit.Measure
            },
            DividedUnit dividedUnit => new UnitViewModel
            {
                Name = dividedUnit.Name,
                UnitNumerator = dividedUnit.UnitNumerator,
                UnitDenominator = dividedUnit.UnitNumerator
            },
            null => null,
            _ => throw new NotSupportedException()
        };
        private static CorporationViewModel MapToViewModel(TimeSeriesAnalysisResult analysis)
        {
            return new CorporationViewModel
            {
                Name = analysis.Corporation.Name,
                CapitalAmount = analysis.Corporation.CapitalAmount,
                IsLinking = analysis.Corporation.IsLinking,
                TypeOfIndustry = analysis.Corporation.TypeOfIndustry
            };
        }
        private static IReadOnlyList<AccountValueViewModel> MapToViewModel(IEnumerable<AccountValue> accountValues)
        {
            return accountValues.Select(x => new AccountValueViewModel
            {
                FinancialAccountPeriod = MapToViewModel(x),
                Amount = x.Amount
            }).ToArray();
        }
        private static AccountsPeriodViewModel MapToViewModel(AccountValue value)
        {
            return value.FinancialAccountPeriod switch
            {
                InstantPeriod instantPeriod => new AccountsPeriodViewModel
                {
                    Instant = instantPeriod.Instant
                },
                DurationPeriod durationPeriod => new AccountsPeriodViewModel
                {
                    From = durationPeriod.From,
                    To = durationPeriod.To
                },
                _ => throw new NotSupportedException()
            };
        }
    }
}
