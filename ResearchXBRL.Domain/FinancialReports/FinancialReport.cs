using ResearchXBRL.Domain.FinancialReportItems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialReports
{
    public sealed class FinancialReport : IReadOnlyList<FinancialReportItem>
    {
        /// <summary>
        ///  企業名
        /// </summary>
        public string CompanyName { get; init; }

        public FinancialReportItem this[int index] => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public IEnumerator<FinancialReportItem> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
