using System.Linq;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReportItems;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReports;
using Xunit;

namespace ResearchXBRL.Tests.Domain.FinancialReports;

public class FinancialReportTests
{
    [Fact]
    public void 報告書内の勘定項目個数を返す()
    {
        // arrange
        var items = new FinancialReportItem[]
        {
                new FinancialReportItem
                {

                },
                new FinancialReportItem
                {

                },
                new FinancialReportItem
                {

                },
        };

        // act
        var report = new FinancialReport(items);

        // assert
        Assert.Equal(items.Length, report.Count);
    }

    [Fact]
    public void LINQを使える()
    {
        // arrange
        var items = new FinancialReportItem[]
        {
                new FinancialReportItem
                {
                    XBRLName = "1"
                },
                new FinancialReportItem
                {
                    XBRLName = "2"
                },
                new FinancialReportItem
                {
                    XBRLName = "3"
                },
        };
        var report = new FinancialReport(items);

        // act
        var expected = string.Join(',', report.Select(x => x.XBRLName));

        // assert
        Assert.Equal(expected, string.Join(',', items.Select(x => x.XBRLName)));
    }
}
