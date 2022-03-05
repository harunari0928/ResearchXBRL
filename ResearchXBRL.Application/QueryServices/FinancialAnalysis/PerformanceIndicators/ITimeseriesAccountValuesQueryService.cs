using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators
{
    public interface ITimeseriesAccountValuesQueryService
    {
        /// <summary>
        /// 指定した企業の時系列会計金額を検索する
        /// </summary>
        /// <param name="corporationId">EDINETの企業ID</param>
        /// <param name="accountItemName">勘定項目の名前</param>
        /// <returns>会計期間と金額の組み合わせ</returns>
        ValueTask<IReadOnlyDictionary<DateOnly, decimal>> Get(string corporationId, string accountItemName);
    }
}
