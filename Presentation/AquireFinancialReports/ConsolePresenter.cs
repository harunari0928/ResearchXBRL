using System;
using Microsoft.Extensions.Logging;
using ResearchXBRL.Application.Usecase.ImportFinancialReports;

namespace AquireFinancialReports.Presenter;

public sealed class ConsolePresenter : IAquireFinancialReportsPresenter
{
    private readonly ILogger<ConsolePresenter> logger;

    public ConsolePresenter(ILogger<ConsolePresenter> logger)
    {
        this.logger = logger;
    }

    public void Complete()
    {
        logger.LogInformation("Aquire reports task is completed.");
    }

    public void Progress(DateTimeOffset start, DateTimeOffset end, DateTimeOffset current)
    {
        var percentage = (current - start).TotalDays / (end - start).TotalDays * 100;
        if (percentage > 100)
        {
            percentage = 100;
        }
        logger.LogInformation($"progress: {percentage:F2}%");
    }

    public void Error(string message, Exception ex)
    {
        logger.LogError(ex, message);
    }

    public void Start()
    {
        logger.LogInformation("Aquire reports task is started.");
    }

    public void Warn(string message)
    {
        logger.LogWarning(message);
    }

    public void Error(string message)
    {
        logger.LogError(message);
    }
}
