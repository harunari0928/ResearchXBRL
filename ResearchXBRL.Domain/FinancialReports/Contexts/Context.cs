using System;
namespace ResearchXBRL.Domain.FinancialReports.Contexts
{
    public sealed class Context
    {
        public string Name { get; init; } = "";
        public IPeriod Period { get; init; } = Activator.CreateInstance<IPeriod>();
    }
}
