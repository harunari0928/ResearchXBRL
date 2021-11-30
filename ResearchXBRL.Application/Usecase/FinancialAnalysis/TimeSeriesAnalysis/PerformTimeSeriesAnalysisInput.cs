namespace ResearchXBRL.Application.Usecase.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class AnalyticalMaterials
    {
        /// <summary>
        /// 企業ID
        /// EDINETの企業コード
        /// </summary>
        /// <value></value>
        public string CorporationId { get; init; } = "";

        /// <summary>
        /// 会計項目名
        /// </summary>
        public string AccountNames { get; init; } = "";
    }
}
