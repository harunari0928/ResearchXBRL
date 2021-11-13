using ResearchXBRL.Domain.FinancialReports;
using ResearchXBRL.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResearchXBRL.Infrastructure.FinancialReports
{
    public sealed class FinancialReportReader : IFinancialReportReader
    {
        private readonly IEdinetXBRLDownloader downloader;
        private readonly IFileStorage fileStorage;

        public FinancialReportReader(
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
