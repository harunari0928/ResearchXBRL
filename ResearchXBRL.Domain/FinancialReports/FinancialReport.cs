using ResearchXBRL.Domain.FinancialReportItems;
using ResearchXBRL.Domain.FinancialReports.Contexts;
using ResearchXBRL.Domain.FinancialReports.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ResearchXBRL.Domain.FinancialReports
{
    public sealed class FinancialReport : IReadOnlyList<FinancialReportItem>
    {
        private readonly FinancialReportItem[] reportItems;

        public FinancialReportItem this[int index] => reportItems[index];

        public int Count => reportItems.Length;

        /// <summary>
        ///  表紙
        /// </summary>
        public ReportCover Cover { get; init; }

        /// <summary>
        /// この報告書で使われている単位一覧
        /// </summary>
        public IReadOnlySet<IUnit> Units { get; init; }

        /// <summary>
        /// この報告書で使われているContext一覧
        /// </summary>
        public IReadOnlySet<Context> Contexts { get; init; }

        public FinancialReport(IEnumerable<FinancialReportItem> reportItems)
        {
            this.reportItems = reportItems.ToArray();
        }

        public IEnumerator<FinancialReportItem> GetEnumerator()
        {
            foreach (var item in reportItems)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in reportItems)
            {
                yield return item;
            }
        }
    }
}
