using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using ResearchXBRL.CrossCuttingInterest.Extensions;
using ResearchXBRL.Domain.FinancialReports;
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
            var files = await GetXBRLFiles(data);
            var xbrlNodes = GetXBRL(files)
                .GetChildNodes()
                .Single(x => x.Name == "xbrli:xbrl")
                .GetChildNodes();
            
            return new FinancialReport
            {
                Cover = new ReportCover
                {
                    CompanyName = xbrlNodes.Single(x => x.Name == "jpcrp_cor:CompanyNameCoverPage").InnerText,
                    DocumentTitle = xbrlNodes.Single(x => x.Name == "jpcrp_cor:DocumentTitleCoverPage").InnerText,
                    SubmissionDate = DateTimeOffset.Parse(xbrlNodes
                            .Single(x => x.Name == "jpcrp_cor:FilingDateCoverPage")
                            .InnerText
                        ?? throw new Exception("提出日時が検出できませんでした")
                    ),
                }
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

        private async Task<IReadOnlyList<string>> GetXBRLFiles(EdinetXBRLData data)
        {
            var zipFilePath = $"./{data.DocumentId}.zip";
            var unzippedFolderPath = $"./{data.DocumentId}";
            fileStorage.Set(data.ZippedDataStream, zipFilePath);
            await data.ZippedDataStream.DisposeAsync();
            fileStorage.Unzip(zipFilePath, unzippedFolderPath);
            return fileStorage
                .GetFiles(Path.Combine(unzippedFolderPath, $"{data.DocumentId}/XBRL/PublicDoc/"),
                "*");
        }
    }
}
