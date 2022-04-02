using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ResearchXBRL.Domain.ImportFinancialReports.Contexts;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReportItems;
using ResearchXBRL.Domain.ImportFinancialReports.Units;

namespace ResearchXBRL.Domain.ImportFinancialReports.FinancialReports
{
    public sealed class FinancialReport : IReadOnlyList<FinancialReportItem>
    {
        private readonly FinancialReportItem[] reportItems;

        public FinancialReportItem this[int index] => reportItems[index];

        public int Count => reportItems.Length;

        /// <summary>
        ///  表紙
        /// </summary>
        public ReportCover Cover { get; init; } = new();

        /// <summary>
        /// この報告書で使われている単位一覧
        /// </summary>
        public IReadOnlySet<IUnit> Units { get; init; } = new HashSet<IUnit>();

        /// <summary>
        /// この報告書で使われているContext一覧
        /// </summary>
        public IReadOnlySet<Context> Contexts { get; init; } = new HashSet<Context>();

        public FinancialReport(IEnumerable<FinancialReportItem> reportItems)
        {
            this.reportItems = reportItems.ToArray();
        }

        public IEnumerator<FinancialReportItem> GetEnumerator()
        {
            var iterator = (this as IEnumerable).GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return iterator.Current as FinancialReportItem
                    ?? throw new NullReferenceException();
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
