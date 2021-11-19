namespace ResearchXBRL.Domain.FinancialReportItems
{
    public sealed class FinancialReportItem
    {
        public string Classification { get; init; }

        /// <summary>
        /// XBRLタグ名
        /// </summary>
        public string XBRLName { get; init; }

        /// <summary>
        /// 数値
        /// </summary>
        public decimal? Amounts { get; init; }

        /// <summary>
        /// 数値の精度
        /// https://www.fsa.go.jp/search/20130821/2b_1.pdf　56ページ参照
        /// </summary>
        public decimal? NumericalAccuracy { get; init; }

        /// <summary>
        /// 数値の表示単位を指定
        /// </summary>
        public decimal? Scale { get; init; }

        /// <summary>
        /// 単位
        /// </summary>
        public string UnitName { get; init; }

        /// <summary>
        /// 詳細
        /// </summary>
        public string ContextName { get; init; }
    }
}
