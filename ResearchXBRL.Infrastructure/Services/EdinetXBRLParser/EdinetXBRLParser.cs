using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;
using ResearchXBRL.Domain.FinancialReports;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ResearchXBRL.Infrastructure.Services.EdinetXBRLParser
{
    public sealed class EdinetXBRLParser : IEdinetXBRLParser
    {
        private readonly IFileStorage fileStorage;

        public EdinetXBRLParser(IFileStorage fileStorage)
        {
            this.fileStorage = fileStorage;
        }

        public FinancialReport Parse(EdinetXBRLData data)
        {
            using var stream = GetXBRLDataStream(data);

            throw new NotImplementedException();
        }

        private async Task<Stream> GetXBRLDataStream(EdinetXBRLData data)
        {
            var zipFilePath = $"/{data.DocumentId}.zip";
            var unzippedFolderPath = $"/{data.DocumentId}";
            fileStorage.Set(data.ZippedDataStream, zipFilePath);
            await data.ZippedDataStream.DisposeAsync();
            fileStorage.Unzip(zipFilePath, unzippedFolderPath);
            var xbrlFilePath = fileStorage.GetFiles(Path.Combine(unzippedFolderPath, "/XBRL/PublicDoc/"), "*.xbrl").Single();
            return fileStorage.Get(xbrlFilePath);
        }
    }
}
