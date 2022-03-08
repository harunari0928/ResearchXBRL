using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.Corporations;

namespace ResearchXBRL.Application.ViewModel.FinancialAnalysis.AnalysisMenus.CorporationMenus;

public sealed class CorporationViewModel
{
    public string Name { get; init; } = "";
    public string CorporationId { get; init; } = "";

    public CorporationViewModel() { }

    public CorporationViewModel(Corporation corporation)
    {
        Name = corporation.Name;
        CorporationId = corporation.CorporationId;
    }
}
