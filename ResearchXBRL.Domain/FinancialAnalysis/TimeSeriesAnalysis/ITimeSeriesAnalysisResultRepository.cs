using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis
{
    public interface ITimeSeriesAnalysisResultRepository
    {
        /// <summary>
        /// 連結財務諸表の時系列分析データを取得する
        /// 単体財務諸表よりも連結財務諸表の方が分析に有用
        /// </summary>
        /// <param name="edinetCode">Edinetの企業コード</param>
        /// <param name="xbrlAccountName">EdinetXBRL固有の会計項目名</param>
        /// <returns>時系列分析データ</returns>
        public Task<TimeSeriesAnalysisResult> GetConsolidateResult(string edinetCode, IEnumerable<string> xbrlAccountName);

        /// <summary>
        /// 単体財務諸表の時系列分析データを取得する
        /// </summary>
        /// <param name="edinetCode">Edinetの企業コード</param>
        /// <param name="xbrlAccountName">EdinetXBRL固有の会計項目名</param>
        /// <returns>時系列分析データ</returns>
        public Task<TimeSeriesAnalysisResult> GetNonConsolidateResult(string edinetCode, IEnumerable<string> xbrlAccountName);
    }
}
