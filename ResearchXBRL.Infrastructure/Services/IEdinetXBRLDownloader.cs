using ResearchXBRL.Infrastructure.Services.EdinetXBRLDownloaders;
using System;
using System.Collections.Generic;

namespace ResearchXBRL.Infrastructure.Services
{
    public interface IEdinetXBRLDownloader
    {
        IAsyncEnumerable<EdinetXBRLData> Download(DateTimeOffset start, DateTimeOffset end);
    }
}
