namespace ResearchXBRL.Application.ViewModel.FinancialAnalysis.TimeSeriesAnalysis;

public class CorporationViewModel
{
    public string Name { get; init; } = "";
    public decimal CapitalAmount { get; init; }
    public bool IsLinking { get; init; }
    public string TypeOfIndustry { get; init; } = "";
}
