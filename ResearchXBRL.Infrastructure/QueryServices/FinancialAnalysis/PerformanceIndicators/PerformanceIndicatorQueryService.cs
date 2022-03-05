using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators.Indicators;
using ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.PerformanceIndicators;

public class PerformanceIndicatorQueryService : IPerformanceIndicatorQueryService, IDisposable, IAsyncDisposable
{
    private readonly NpgsqlConnection connection;

    public PerformanceIndicatorQueryService()
    {
        var server = Environment.GetEnvironmentVariable("DB_SERVERNAME");
        var userId = Environment.GetEnvironmentVariable("DB_USERID");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");
        var port = Environment.GetEnvironmentVariable("DB_PORT");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var connectionString = $"Server={server};Port={port};Database={dbName};User Id={userId};Password={password};Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100";
        connection = new NpgsqlConnection(connectionString);
        connection.Open();
    }

    public void Dispose()
    {
        connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await connection.DisposeAsync();
    }

    public async ValueTask<PerformanceIndicator> Get(string corporationId)
    {
        return new Application.DTO.FinancialAnalysis.PerformanceIndicators.PerformanceIndicator
        {
            Indicators = await GetIndicators(corporationId).ToListAsync()
        };
    }

    private async IAsyncEnumerable<Indicator> GetIndicators(string corporationId)
    {
        foreach (var indicatorType in Enum.GetValues<IndicatorType>())
        {
            yield return await GetIndicator(corporationId, indicatorType);
        }
    }
    private async ValueTask<Indicator> GetIndicator(string corporationId, IndicatorType indicatorType)
    {
        using var command = connection.CreateCommand();
        SetSQLQuery(command, corporationId, indicatorType);
        using var reader = await command.ExecuteReaderAsync();
        return new Indicator
        {
            IndicatorType = indicatorType,
            Values = await ReadIndicatorValues(reader)
        };
    }
    private static async ValueTask<IReadOnlyDictionary<DateOnly, decimal>> ReadIndicatorValues(NpgsqlDataReader reader)
    {
        var amountsIndex = reader.GetOrdinal("amounts");
        var periodToIndex = reader.GetOrdinal("period_to");
        var instantDateIndex = reader.GetOrdinal("instant_date");
        var values = new Dictionary<DateOnly, decimal>();
        while (await reader.ReadAsync())
        {
            var date = GetIndicatorDate(reader, periodToIndex, instantDateIndex);
            if (values.ContainsKey(date))
            {
                continue;
            }
            values.Add(date, (decimal)reader[amountsIndex]);
        }
        return values;
    }
    private static DateOnly GetIndicatorDate(NpgsqlDataReader reader, int periodToIndex, int instantDateIndex)
    {
        var dateString = (reader[periodToIndex] is DBNull ? reader[instantDateIndex] : reader[periodToIndex]).ToString();
        if (dateString is null)
        {
            throw new NullReferenceException("日付データがnullです");
        }

        return DateOnly.FromDateTime(DateTime.Parse(dateString));
    }
    private static void SetSQLQuery(NpgsqlCommand command, string corporationId, IndicatorType indicatorType)
    {
        command.CommandText = @"     
SELECT
    A.amounts,
    C.period_to,
    C.instant_date,
    COALESCE(E.priority_of_use, 0) priority_of_use
FROM
    report_items A
INNER JOIN
    contexts C
ON
    A.report_id = C.report_id
AND
    A.context_name = c.context_name
INNER JOIN
    report_covers RC
ON
    A.report_id = RC.id
INNER JOIN
    company_master D
ON
    RC.company_id = D.code
AND
    C.context_name IN ('CurrentYearInstant', 'CurrentYearDuration')
LEFT OUTER JOIN
    aggregation_of_names_list E
ON
    A.xbrl_name = E.aggregate_target
WHERE
    (A.xbrl_name = @XBRLName OR E.aggregate_result = @XBRLName)
AND
    D.code = @corporationId
GROUP BY
    A.amounts,
    C.period_to,
    C.instant_date,
    priority_of_use
HAVING
    priority_of_use = MIN(priority_of_use)
ORDER BY
    period_to, instant_date;
";
        command.Parameters.Add("@corporationId", NpgsqlDbType.Varchar)
            .Value = corporationId;
        command.Parameters.Add("@XBRLName", NpgsqlDbType.Varchar)
            .Value = ToXBRLName(indicatorType);
    }
    private static string ToXBRLName(IndicatorType indicatorType)
    {
        return indicatorType switch
        {
            IndicatorType.NetSales => "NetSales",
            IndicatorType.OperatingIncome => "OperatingIncome",
            IndicatorType.OrdinaryIncome => "OrdinaryIncome",
            IndicatorType.ProfitLossAttributableToOwnersOfParent => "ProfitLossAttributableToOwnersOfParent",
            IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults => "RateOfReturnOnEquitySummaryOfBusinessResults",
            IndicatorType.DividendPaidPerShareSummaryOfBusinessResults => "DividendPaidPerShareSummaryOfBusinessResults",
            _ => throw new NotSupportedException()
        };
    }
}
