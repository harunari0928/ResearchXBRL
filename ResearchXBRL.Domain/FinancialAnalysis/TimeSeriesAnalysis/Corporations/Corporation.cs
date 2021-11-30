using System;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations
{
    public sealed class Corporation
    {
        /// <summary>
        /// 企業名
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// 資本金
        /// </summary>
        public decimal CapitalAmount { get; init; }

        /// <summary>
        /// 連結の有無
        /// </summary>
        public bool IsLinking { get; init; }

        /// <summary>
        /// 業種
        /// </summary>
        public string TypeOfIndustry { get; init; } = "";
    }
}
