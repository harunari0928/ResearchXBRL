using ResearchXBRL.Domain.FinancialReports;
using ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Tests.Infrastructure.FinancialReports
{
    public sealed class FinancialReportDownloader : IFinancialReportReader
    {
        private readonly IEdinetXBRLDownloader downloader;
        private readonly IFileStorage fileStorage;

        public FinancialReportDownloader(
            IEdinetXBRLDownloader downloader,
            IFileStorage fileStorage)
        {
            this.downloader = downloader;
            this.fileStorage = fileStorage;
        }

        public async Task<IEnumerable<FinancialReport>> Read(DateTimeOffset start, DateTimeOffset end)
        {
            await foreach (var data in downloader.Download(start, end))
            {
                fileStorage.Set(data.BinaryData, data.DocumentId);
            }
            
            throw new NotImplementedException();
        }        
    }
}
