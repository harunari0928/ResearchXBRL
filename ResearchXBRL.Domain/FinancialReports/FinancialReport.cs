using ResearchXBRL.Domain.FinancialReportItems;
using ResearchXBRL.Domain.FinancialReports.Details;
using ResearchXBRL.Domain.FinancialReports.Units;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ResearchXBRL.Domain.FinancialReports
{
    public sealed class FinancialReport : IReadOnlyList<FinancialReportItem>
    {
        /// <summary>
        ///  表紙
        /// </summary>
        public ReportCover Cover { get; init; }

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

        /// <summary>
        /// この報告書で使われている単位一覧
        /// </summary>
        public IReadOnlySet<IUnit> Units { get; init; }

        /// <summary>
        /// この報告書で使われているContext一覧
        /// </summary>
        public IReadOnlySet<Context> Contexts { get; init; }
    }
}
