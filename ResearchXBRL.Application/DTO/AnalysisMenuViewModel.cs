using System.Collections.Generic;

namespace ResearchXBRL.Application.DTO
{
    public class AnalysisMenuViewModel
    {
        IReadOnlyList<string> AccountItems { get; init; } = new string[0];
        IReadOnlyList<string> Corporations { get; init; } = new string[0];
    }
}
