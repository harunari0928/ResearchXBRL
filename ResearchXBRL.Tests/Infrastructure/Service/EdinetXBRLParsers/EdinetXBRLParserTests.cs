using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Domain.ImportFinancialReports.Contexts;
using ResearchXBRL.Domain.ImportFinancialReports.FinancialReports;
using ResearchXBRL.Domain.ImportFinancialReports.Units;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLParser;
using ResearchXBRL.Infrastructure.Shared.FileStorages;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service.EdinetXBRLParsers;

public sealed class EdinetXBRLParserTests
{
    public sealed class ParseTests : IDisposable
    {
        private readonly string documentId = "S100AJZW";
        private readonly string companyId = "test";
        private readonly string documentType = "testtype";
        private readonly LocalFileStorage storage = new("./work");

        public ParseTests()
        {
            if (Directory.Exists("work"))
            {
                Directory.Delete("work", true);
            }
        }

        [Fact]
        public async Task 単位を全表紙情報を取得する()
        {
            // arrange & act
            var report = await CreateReport();

            // assert
            Assert.Equal(documentId, report.Cover.DocumentId);
            Assert.Equal(documentType, report.Cover.DocumentType);
            Assert.Equal(companyId, report.Cover.CompanyId);
            Assert.Equal("jppfs", report.Cover.AccountingStandards);
            Assert.Equal("2017-06-23", $"{report.Cover.SubmissionDate:yyyy-MM-dd}");
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
            Assert.Equal(104, report.Contexts.Count);

            // よくあるコンテキスト
            var firstContext = report.Contexts.Single(x => x.Name == "CurrentYearInstant");
            Assert.IsType<InstantPeriod>(firstContext.Period);
            if (firstContext.Period is InstantPeriod instantPeriod1)
            {
                Assert.Equal("2017-03-31", $"{instantPeriod1.InstantDate:yyyy-MM-dd}");
            }

            // 期間のコンテキスト
            var intermediateContext = report.Contexts.Single(x => x.Name == "Prior1YearDuration_NonConsolidatedMember_SubscriptionRightsToSharesMember");
            Assert.IsType<DurationPeriod>(intermediateContext.Period);
            if (intermediateContext.Period is DurationPeriod durationPeriod1)
            {
                Assert.Equal("2015-04-01", $"{durationPeriod1.From:yyyy-MM-dd}");
                Assert.Equal("2016-03-31", $"{durationPeriod1.To:yyyy-MM-dd}");
            }

            // 一番最後のコンテキスト
            var lastContext = report.Contexts.Single(x => x.Name == "Prior4YearDuration_NonConsolidatedMember");
            Assert.IsType<DurationPeriod>(lastContext.Period);
            if (firstContext.Period is DurationPeriod durationPeriod2)
            {
                Assert.Equal("2012-04-01", $"{durationPeriod2.From:yyyy-MM-dd}");
                Assert.Equal("2013-03-31", $"{durationPeriod2.To:yyyy-MM-dd}");
            }
        }

        [Fact]
        public async Task 勘定科目を全て取得する()
        {
            // arrange & act
            var report = await CreateReport();

            // assert
            Assert.Equal(234, report.Where(x => x.Classification == "jpcrp").Count());
            Assert.Equal(365, report.Where(x => x.Classification == "jppfs").Count());
            Assert.Equal(12, report.Where(x => x.Classification.StartsWith("jpcrp030000-asr")).Count());
            Assert.Equal(234 + 365 + 12, report.Count);

            // 最初の勘定科目
            var first = report.Where(x => x.Classification == "jppfs").First();
            Assert.Equal("jppfs", first.Classification);
            Assert.Equal("CashAndDeposits", first.XBRLName);
            Assert.Equal("Prior1YearInstant_NonConsolidatedMember", first.ContextName);
            Assert.Equal("JPY", first.UnitName);
            Assert.Equal(-6, first.NumericalAccuracy);
            Assert.Equal(83078000000, first.Amounts);
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

            Assert.Null(storage.Get($"/{documentId}.zip"));
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
                LazyZippedDataStream = new Lazy<Task<IResult<MemoryStream>>>(async () =>
                {
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    return new Succeeded<MemoryStream>(memoryStream);
                }, true)
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
