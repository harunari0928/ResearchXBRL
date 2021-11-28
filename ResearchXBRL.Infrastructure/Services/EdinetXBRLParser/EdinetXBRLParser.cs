﻿using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using ResearchXBRL.CrossCuttingInterest.Extensions;
using ResearchXBRL.Domain.FinancialReportItems;
using ResearchXBRL.Domain.FinancialReports;
using ResearchXBRL.Domain.FinancialReports.Contexts;
using ResearchXBRL.Domain.FinancialReports.Units;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLParser
{
    public sealed class EdinetXBRLParser : IEdinetXBRLParser
    {
        private readonly IFileStorage fileStorage;

        public EdinetXBRLParser(IFileStorage fileStorage)
        {
            this.fileStorage = fileStorage;
        }

        public async Task<FinancialReport> Parse(EdinetXBRLData data)
        {
            var (directory, files) = await GetXBRLFiles(data);
            var xbrlNodes = GetXBRL(files)
                .GetChildNodes()
                .Single(x => x.Name == "xbrli:xbrl")
                .GetChildNodes();

            var accountingStandards = GetAccountingStandards(files);

            fileStorage.Delete(directory);

            return new FinancialReport(CreateReportItems(xbrlNodes))
            {
                Cover = CreateReortCover(data, xbrlNodes, accountingStandards),
                Units = CreateUnits(xbrlNodes),
                Contexts = CreateContexts(xbrlNodes),
            };
        }

        private static IEnumerable<FinancialReportItem> CreateReportItems(IEnumerable<XmlNode> xbrlNodes)
        {
            return xbrlNodes
                .Where(x => x.GetAttributeValue("unitRef") is not null)
                .Where(x => x.Name.StartsWith("jpcrp_cor:")
                 || x.Name.StartsWith("jppfs_cor:")
                 || x.Name.StartsWith("jpigp_cor:"))
                .Select(CreateReportItem);
        }

        private static FinancialReportItem CreateReportItem(XmlNode node)
        {
            var accuraryValue = node.GetAttributeValue("decimals");
            var scaleValue = node.GetAttributeValue("scale");
            return new FinancialReportItem
            {
                Classification = node.Name.Split(':')[0],
                XBRLName = node.Name.Split(':')[1],
                ContextName = node.GetAttributeValue("contextRef"),
                UnitName = node.GetAttributeValue("unitRef"),
                NumericalAccuracy = accuraryValue is null ? null : decimal.Parse(accuraryValue),
                Amounts = string.IsNullOrWhiteSpace(node.InnerText) ? null : decimal.Parse(node.InnerText),
                Scale = scaleValue is null ? null : decimal.Parse(scaleValue),
            };
        }

        private static IReadOnlySet<IUnit> CreateUnits(IEnumerable<XmlNode> xbrlNodes)
        {
            return xbrlNodes
                .Where(x => x.Name == "xbrli:unit")
                .Select(CreateUnit)
                .ToHashSet();
        }

        private static HashSet<Context> CreateContexts(IEnumerable<XmlNode> xbrlNodes)
        {
            return xbrlNodes
                .Where(x => x.Name == "xbrli:context")
                .Select(CreateContext)
                .ToHashSet();
        }

        private static ReportCover CreateReortCover(EdinetXBRLData data, IEnumerable<XmlNode> xbrlNodes, string accountingStandards)
        {
            return new ReportCover
            {
                DocumentId = data.DocumentId,
                CompanyId = data.CompanyId,
                DocumentType = data.DocumentType,
                AccountingStandards = accountingStandards,
                SubmissionDate = DateTimeOffset.Parse(xbrlNodes
                                        .Single(x => x.Name == "jpcrp_cor:FilingDateCoverPage")
                                        .InnerText
                                    ?? throw new Exception("提出日時が検出できませんでした")
                                ),
            };
        }

        private XmlDocument GetXBRL(IReadOnlyList<string> files)
        {
            var xbrlFile = files.Single(x => x.EndsWith(".xbrl"));
            using var xbrlFileStream = fileStorage.Get(xbrlFile);
            var xbrl = new XmlDocument();
            xbrl.Load(xbrlFileStream);
            return xbrl;
        }

        private string GetAccountingStandards(IReadOnlyList<string> files)
        {
            var xbrlFile = files.Single(x => x.EndsWith(".xsd"));
            using var xbrlFileStream = fileStorage.Get(xbrlFile);
            using var streamReader = new StreamReader(xbrlFileStream);
            var isJppfs = streamReader.ReadToEnd().Contains("jppfs");
            return isJppfs ? "jppfs" : "jpigp";
        }

        private static IUnit CreateUnit(XmlNode node)
        {
            var child = node.FirstChild ?? throw new Exception("単位タグが不正です");
            var unitName = node.GetAttributeValue("id");
            if (child.Name == "xbrli:divide")
            {
                return new DividedUnit
                {
                    Name = unitName,
                    UnitNumerator = child
                        .GetChildNodes()
                        .Single(x => x.Name == "xbrli:unitNumerator")
                        .FirstChild?.InnerText,
                    UnitDenominator = child
                        .GetChildNodes()
                        .Single(x => x.Name == "xbrli:unitDenominator")
                        .FirstChild?.InnerText
                };
            }

            return new NormalUnit
            {
                Name = unitName,
                Measure = child.InnerText
            };
        }

        private static Context CreateContext(XmlNode node)
        {
            var child = node.GetChildNodes().Single(x => x.Name == "xbrli:period");
            var contextName = node.GetAttributeValue("id");
            return new Context
            {
                Name = contextName,
                Period = CreatePeriod(child)
            };
        }

        private static IPeriod CreatePeriod(XmlNode node)
        {
            if (node.FirstChild?.Name == "xbrli:instant")
            {
                return new InstantPeriod
                {
                    InstantDate = DateTimeOffset.Parse($"{node.FirstChild?.InnerText} +09:00")
                };
            }

            var startEndElements = node.GetChildNodes();
            return new DurationPeriod
            {
                From = DateTimeOffset.Parse($"{startEndElements.Single(x => x.Name == "xbrli:startDate").InnerText} +09:00"),
                To = DateTimeOffset.Parse($"{startEndElements.Single(x => x.Name == "xbrli:endDate").InnerText} +09:00"),
            };
        }

        private async Task<(string directory, IReadOnlyList<string> files)> GetXBRLFiles(EdinetXBRLData data)
        {
            var zipFilePath = $"./{data.DocumentId}.zip";
            var unzippedFolderPath = $"./{data.DocumentId}";
            fileStorage.Set(data.ZippedDataStream, zipFilePath);
            fileStorage.Unzip(zipFilePath, unzippedFolderPath);
            fileStorage.Delete(zipFilePath);
            await data.DisposeAsync();
            return (unzippedFolderPath, fileStorage
                .GetFiles(Path.Combine(unzippedFolderPath, $"XBRL/PublicDoc/"),
                "*"));
        }
    }
}
