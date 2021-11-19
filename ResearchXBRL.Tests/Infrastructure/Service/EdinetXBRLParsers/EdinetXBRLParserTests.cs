using ResearchXBRL.Domain.FinancialReports;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLParser;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service.EdinetXBRLParsers
{
    public sealed class EdinetXBRLParserTests
    {
        public sealed class ParseTests : IDisposable
        {
            [Fact]
            public async Task 表紙情報を取得できる()
            {
                var report = await CreateReport();
                Assert.Equal("有価証券報告書", report.Cover.DocumentTitle);
                Assert.Equal("株式会社ファーマフーズ", report.Cover.CompanyName);
                Assert.Equal("2021-10-20", $"{report.Cover.SubmissionDate:yyyy-MM-dd}");
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
