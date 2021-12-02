using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis
{
    public interface ITimeSeriesAnalysisResultRepository
    {
        /// <summary>
        /// 連結財務諸表の時系列分析データを取得する
        /// 単体財務諸表よりも連結財務諸表の方が分析に有用
        /// </summary>
        /// <param name="corporationId">企業コード</param>
        /// <param name="accountItemName">会計項目名</param>
        /// <returns>時系列分析データ</returns>
        public Task<TimeSeriesAnalysisResult> GetConsolidateResult(string corporationId, string accountItemName);

        /// <summary>
        /// 単体財務諸表の時系列分析データを取得する
        /// </summary>
        /// <param name="corporationId">企業コード</param>
        /// <param name="accountItemName">会計項目名</param>
        /// <returns>時系列分析データ</returns>
        public Task<TimeSeriesAnalysisResult> GetNonConsolidateResult(string corporationId, string accountItemName);
    }
}
