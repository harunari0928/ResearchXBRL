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

        public void Progress(double percentage)
        {
            Console.WriteLine($"progress: {percentage:F2}%");
        }

        public void Error(string message, Exception ex)
        {
            Console.WriteLine($"message:{message}{Environment.NewLine}{ex}");
        }
    }
}
