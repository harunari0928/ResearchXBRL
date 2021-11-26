using ResearchXBRL.Domain.FinancialReports;
using ResearchXBRL.Domain.FinancialReports.Contexts;
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
            private readonly string documentId = "S100MMP3";
            private readonly string companyId = "test";
            private readonly string documentType = "testtype";
            private readonly LocalStorage storage = new("./work");

            public ParseTests()
            {
                if (Directory.Exists("work"))
                {
                    Directory.Delete("work", true);
                }
            }

            [Fact]
            public async Task 表紙情報を取得する()
            {
                // arrange & act
                var report = await CreateReport();

                // assert
                Assert.Equal(documentId, report.Cover.DocumentId);
                Assert.Equal(documentType, report.Cover.DocumentType);
                Assert.Equal(companyId, report.Cover.CompanyId);
                Assert.Equal("jppfs", report.Cover.AccountingStandards);
                Assert.Equal("2021-10-20", $"{report.Cover.SubmissionDate:yyyy-MM-dd}");
            }

            [Fact]
            public async Task 単位を全て取得する()
            {
                // arrange & act
                var report = await CreateReport();

                // assert
                var jpyPerShares = report.Units.Single(x => x.Name == "JPYPerShares");
                Assert.IsType<DividedUnit>(jpyPerShares);
                if (jpyPerShares is DividedUnit divided1)
                {
                    Assert.Equal("iso4217:JPY", divided1.UnitNumerator);
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

            [Fact]
            public async Task コンテキスト情報を全て取得する()
            {
                // arrange & act
                var report = await CreateReport();

                // assert
                Assert.Equal(195, report.Contexts.Count);

                // 一番先頭のコンテキスト
                var firstContext = report.Contexts.Single(x => x.Name == "FilingDateInstant_jpcrp030000-asr_E02484-000MasudaKazuyukiMember");
                Assert.IsType<InstantPeriod>(firstContext.Period);
                if (firstContext.Period is InstantPeriod instantPeriod1)
                {
                    Assert.Equal("2021-10-20", $"{instantPeriod1.InstantDate:yyyy-MM-dd}");
                }

                // 中間のコンテキスト
                var intermediateContext = report.Contexts.Single(x => x.Name == "Prior1YearDuration_NonConsolidatedMember_CapitalStockMember");
                Assert.IsType<DurationPeriod>(intermediateContext.Period);
                if (intermediateContext.Period is DurationPeriod durationPeriod1)
                {
                    Assert.Equal("2019-08-01", $"{durationPeriod1.From:yyyy-MM-dd}");
                    Assert.Equal("2020-07-31", $"{durationPeriod1.To:yyyy-MM-dd}");
                }

                // 一番最後のコンテキスト
                var lastContext = report.Contexts.Single(x => x.Name == "FilingDateInstant_jpcrp030000-asr_E02484-000UedaTaroMember");
                Assert.IsType<InstantPeriod>(lastContext.Period);
                if (firstContext.Period is InstantPeriod instantPeriod2)
                {
                    Assert.Equal("2021-10-20", $"{instantPeriod2.InstantDate:yyyy-MM-dd}");
                }
            }

            [Fact]
            public async Task 勘定科目を全て取得する()
            {
                // arrange & act
                var report = await CreateReport();

                // assert
                Assert.Equal(397, report.Where(x => x.Classification == "jpcrp_cor").Count());
                Assert.Equal(688, report.Where(x => x.Classification == "jppfs_cor").Count());
                Assert.Equal(397 + 688, report.Count);

                // 最初の勘定科目
                var first = report[0];
                Assert.Equal("jpcrp_cor", first.Classification);
                Assert.Equal("NetSalesSummaryOfBusinessResults", first.XBRLName);
                Assert.Equal("Prior4YearDuration", first.ContextName);
                Assert.Equal("JPY", first.UnitName);
                Assert.Equal(-6, first.NumericalAccuracy);
                Assert.Equal(4722000000, first.Amounts);
                Assert.Null(first.Scale);
            }

            [Fact]
            public async Task Zipファイルを削除する()
            {
                // arrange & act
                await CreateReport();

                Assert.False(Directory.Exists($"./work/{documentId}"));
            }

            [Fact]
            public async Task Zip解凍後フォルダを削除する()
            {
                // arrange & act
                await CreateReport();

                Assert.Throws<FileNotFoundException>(() => storage.Get($"/{documentId}.zip"));
            }

            private async Task<FinancialReport> CreateReport()
            {
                using var stream = new FileStream($"{documentId}.zip", FileMode.Open);
                var parser = new EdinetXBRLParser(storage);
                return await parser.Parse(new ResearchXBRL.Application.DTO.EdinetXBRLData
                {
                    DocumentId = documentId,
                    CompanyId = companyId,
                    DocumentType = documentType,
                    ZippedDataStream = stream
                });
            }

            public void Dispose()
            {
                if (Directory.Exists("work"))
                {
                    Directory.Delete("work", true);
                }
            }
        }
    }
}
