using System;
using ResearchXBRL.Application.Usecase.FinancialReports;

namespace AquireFinancialReports
{
    public sealed class ConsolePresenter : IAquireFinancialReportsPresenter
    {
        public void Complete()
        {
            Console.WriteLine("Aquire reportsTask is completed.");
        }

        public void Progress(int percentage)
        {
            Console.WriteLine($"progress: {percentage}%");
        }
    }
}
