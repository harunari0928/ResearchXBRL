using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.DTO.Results;
using System;
using System.Collections.Generic;

namespace ResearchXBRL.Application.Services
{
    public interface IEdinetXBRLDownloader
    {
        IAsyncEnumerable<IResult<EdinetXBRLData>> Download(DateTimeOffset start, DateTimeOffset end);
    }
}
