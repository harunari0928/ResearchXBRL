using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;

namespace ResearchXBRL.Infrastructure.QueryServices.FinancialAnalysis.PerformanceIndicators;

public class TimeserieAccountValuesQueryService : ITimeseriesAccountValuesQueryService
{
    private readonly NpgsqlConnection connection;

    public TimeserieAccountValuesQueryService()
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

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<DateOnly, decimal>> Get(string corporationId, string accountItemName)
    {
        using var command = connection.CreateCommand();
        SetSQLQuery(corporationId, accountItemName, command);
        using var reader = await command.ExecuteReaderAsync();
        return await ReadValues(reader);
    }

    private static void SetSQLQuery(string corporationId, string accountItemName, NpgsqlCommand command)
    {
        command.CommandText = @"
SELECT
    A.amounts,
    C.period_to,
    C.instant_date
FROM
    report_items A
INNER JOIN (
    SELECT
        xbrl_name,
        MAX(account_name) AS account_name
    FROM
        account_elements
    GROUP BY
        xbrl_name
) B
ON
    A.xbrl_name = B.xbrl_name
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
AND
    D.code = @corporationId
AND
    B.account_name = @accountName;
";
        command.Parameters.Add("@corporationId", NpgsqlDbType.Varchar)
                .Value = corporationId;
        command.Parameters.Add("@accountName", NpgsqlDbType.Varchar)
            .Value = accountItemName;
    }
    private static async ValueTask<IReadOnlyDictionary<DateOnly, decimal>> ReadValues(NpgsqlDataReader reader)
    {
        var amountsIndex = reader.GetOrdinal("amounts");
        var periodToIndex = reader.GetOrdinal("period_to");
        var instantDateIndex = reader.GetOrdinal("instant_date");
        var values = new Dictionary<DateOnly, decimal>();
        while (await reader.ReadAsync())
        {
            var date = GetDate(reader, periodToIndex, instantDateIndex);
            if (values.ContainsKey(date))
            {
                continue;
            }
            values.Add(date, (decimal)reader[amountsIndex]);
        }
        return values;
    }
    private static DateOnly GetDate(NpgsqlDataReader reader, int periodToIndex, int instantDateIndex)
    {
        var dateString = (reader[periodToIndex] is DBNull ? reader[instantDateIndex] : reader[periodToIndex]).ToString();
        if (dateString is null)
        {
            throw new NullReferenceException("日付データがnullです");
        }

        return DateOnly.FromDateTime(DateTime.Parse(dateString));
    }
}
