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

public class PerformanceIndicatorsQueryService : IPerformanceIndicatorsQueryService, IDisposable, IAsyncDisposable
{
    private readonly NpgsqlConnection connection;

    public PerformanceIndicatorsQueryService()
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
        SetSQLQuery(command, GetQueryBuilder(indicatorType, corporationId));
        using var reader = await command.ExecuteReaderAsync();
        return new Indicator
        {
            IndicatorType = indicatorType,
            Values = await ReadIndicatorValues(reader)
        };
    }
    private static IQueryBuilder GetQueryBuilder(IndicatorType indicatorType, string corporationId)
    {
        return indicatorType switch
        {
            IndicatorType.DividendPaidPerShareSummaryOfBusinessResults => new DividendPaidPerShareSummaryOfBusinessResultsQueryBuilder
            {
                CorporationId = corporationId
            },
            _ => new CommonQueryBuilder(indicatorType, corporationId)
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
    private static void SetSQLQuery(NpgsqlCommand command, IQueryBuilder queryBuilder)
    {
        command.CommandText = queryBuilder.Query;
        command.Parameters.Add("@corporationId", NpgsqlDbType.Varchar)
            .Value = queryBuilder.CorporationId;
        command.Parameters.Add("@XBRLName", NpgsqlDbType.Varchar)
            .Value = queryBuilder.XBRLName;
    }
    private interface IQueryBuilder
    {
        string XBRLName { get; }
        string CorporationId { get; }
        string Query { get; }
    }
    private sealed class CommonQueryBuilder : IQueryBuilder
    {
        private readonly IndicatorType indicatorType;
        private readonly string corporationId;

        public string XBRLName => ToXBRLName(indicatorType);

        public string CorporationId => corporationId;

        public CommonQueryBuilder(IndicatorType indicatorType, string corporationId)
        {
            this.indicatorType = indicatorType;
            this.corporationId = corporationId;
        }

        public string Query => @"
SELECT
    *
FROM (    
    SELECT
        A.amounts,
        C.period_to,
        C.instant_date,
        COALESCE(E.priority_of_use, 0) AS priority_of_use,
        MIN(COALESCE(E.priority_of_use, 0)) OVER (PARTITION BY COALESCE(C.period_to, C.instant_date)) AS min_priority_of_use
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
    AND
        A.amounts IS NOT NULL
    GROUP BY
        A.amounts,
        C.period_to,
        C.instant_date,
        E.priority_of_use
    ORDER BY
        period_to, instant_date
) A
WHERE
    priority_of_use = min_priority_of_use;
";
        private static string ToXBRLName(IndicatorType indicatorType)
        {
            return indicatorType switch
            {
                IndicatorType.NetSales => "NetSales",
                IndicatorType.OperatingIncome => "OperatingIncome",
                IndicatorType.OrdinaryIncome => "OrdinaryIncome",
                IndicatorType.ProfitLossAttributableToOwnersOfParent => "ProfitLossAttributableToOwnersOfParent",
                IndicatorType.RateOfReturnOnEquitySummaryOfBusinessResults => "RateOfReturnOnEquitySummaryOfBusinessResults",
                _ => throw new NotSupportedException()
            };
        }
    }
    private sealed class DividendPaidPerShareSummaryOfBusinessResultsQueryBuilder : IQueryBuilder
    {
        public string XBRLName => "DividendPaidPerShareSummaryOfBusinessResults";

        public string CorporationId { get; init; } = "";

        public string Query => @"
SELECT
    *
FROM (    
    SELECT
        A.amounts,
        C.period_to,
        C.instant_date,
        COALESCE(E.priority_of_use, 0) AS priority_of_use,
        MIN(COALESCE(E.priority_of_use, 0)) OVER (PARTITION BY COALESCE(C.period_to, C.instant_date)) AS min_priority_of_use
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
        C.context_name = 'CurrentYearDuration_NonConsolidatedMember'
    LEFT OUTER JOIN
        aggregation_of_names_list E
    ON
        A.xbrl_name = E.aggregate_target
    WHERE
        (A.xbrl_name = @XBRLName OR E.aggregate_result = @XBRLName)
    AND
        D.code = @corporationId
    AND
        A.amounts IS NOT NULL
    GROUP BY
        A.amounts,
        C.period_to,
        C.instant_date,
        E.priority_of_use
    ORDER BY
        period_to, instant_date
) A
WHERE
    priority_of_use = min_priority_of_use;
";
    }
}
