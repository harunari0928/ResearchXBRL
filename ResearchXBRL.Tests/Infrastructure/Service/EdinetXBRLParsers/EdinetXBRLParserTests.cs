using ResearchXBRL.Domain.FinancialReports;
using ResearchXBRL.Domain.FinancialReports.Units;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLParser;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service.EdinetXBRLParsers
{
    public sealed class EdinetXBRLParserTests
    {
        public sealed class ParseTests : IDisposable
        {
            [Fact]
            public async Task 表紙情報を取得する()
            {
                // arrange & act
                var report = await CreateReport();

                // assert
                Assert.Equal("有価証券報告書", report.Cover.DocumentTitle);
                Assert.Equal("株式会社ファーマフーズ", report.Cover.CompanyName);
                Assert.Equal("2021-10-20", $"{report.Cover.SubmissionDate:yyyy-MM-dd}");
            }

            [Fact]
            public async Task 単位をすべて取得する()
            {
                // arrange & act
                var report = await CreateReport();

                // assert
                var jpyPerShares = report.Units.Single(x => x.Name == "JPYPerShares");
                Assert.IsType<DividedUnit>(jpyPerShares);
                if (jpyPerShares is DividedUnit divided1)
                {
                    Assert.Equal("iso4217:JPY", divided1.UnitNumeratorMeasure);
                    Assert.Equal("xbrli:shares", divided1.UnitDenominator);
                }

                var shares = report.Units.Single(x => x.Name == "shares");
                Assert.IsType<NormalUnit>(shares);
                if (jpyPerShares is NormalUnit normal1)
                {
                    Assert.Equal("xbrli:shares", normal1.Measure);
                }

                var pure = report.Units.Single(x => x.Name == "pure");
                Assert.IsType<NormalUnit>(pure);
                if (jpyPerShares is NormalUnit normal2)
                {
                    Assert.Equal("xbrli:pure", normal2.Measure);
                }

                var jpy = report.Units.Single(x => x.Name == "JPY");
                Assert.IsType<NormalUnit>(jpy);
                if (jpyPerShares is NormalUnit normal3)
                {
                    Assert.Equal("iso4217:JPY", normal3.Measure);
                }
            }

            private static async Task<FinancialReport> CreateReport()
            {
                using var stream = new FileStream("XBRLParserTest.zip", FileMode.Open);
                var parser = new EdinetXBRLParser(new LocalStorage("./"));
                return await parser.Parse(new ResearchXBRL.Application.DTO.EdinetXBRLData
                {
                    DocumentId = "S100MMP3",
                    ZippedDataStream = stream
                });
            }

            public void Dispose()
            {
                Directory.Delete("S100MMP3", true);
            }
        }
    }
}
