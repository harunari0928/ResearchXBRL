using ResearchXBRL.Domain.FinancialReportItems;
using ResearchXBRL.Domain.FinancialReports.Contexts;
using ResearchXBRL.Domain.FinancialReports.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ResearchXBRL.Domain.FinancialReports
{
    public sealed class FinancialReport : IReadOnlyCollection<FinancialReportItem>
    {
        private readonly FinancialReportItem[] reportedAccountItems;
        private readonly FinancialReportItem[] cabinetOfficeOrdinanceItems;
        
        public int Count => reportedAccountItems.Length + cabinetOfficeOrdinanceItems.Length;

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

        public FinancialReport(
            IEnumerable<FinancialReportItem> reportedAccountItems,
            IEnumerable<FinancialReportItem> reportedCabinetOfficeOrdinanceItems)
        {
            this.reportedAccountItems = reportedAccountItems.ToArray();
            this.cabinetOfficeOrdinanceItems = reportedCabinetOfficeOrdinanceItems.ToArray();
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
            foreach (var item in reportedAccountItems)
            {
                yield return item;
            }
        }
    }
}
