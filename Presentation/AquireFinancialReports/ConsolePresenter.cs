using System;
using ResearchXBRL.Application.Usecase.ImportFinancialReports;

namespace AquireFinancialReports
{
    public sealed class ConsolePresenter : IAquireFinancialReportsPresenter
    {
        public void Complete()
        {
            Console.WriteLine("Aquire reportsTask is completed.");
        }

        public void Progress(DateTimeOffset start, DateTimeOffset end, DateTimeOffset current)
        {
            var percentage = (current - start).TotalDays / (end - start).TotalDays * 100;
            Console.WriteLine($"progress: {percentage:F2}%");
        }

        public void Error(string message, Exception ex)
        {
            Console.WriteLine($"message:{message}{Environment.NewLine}{ex}");
        }
    }
}
