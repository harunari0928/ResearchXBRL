using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.AccountPeriods;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Units;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.TimeSeriesAnalysis
{
    public sealed class TimeSeriesAnalysisResultRepository : ITimeSeriesAnalysisResultRepository, IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlConnection connection;
        private readonly ICorporationRepository corporationRepository;

        public TimeSeriesAnalysisResultRepository(ICorporationRepository corporationRepository)
        {
            this.corporationRepository = corporationRepository;
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

        public async Task<TimeSeriesAnalysisResult> GetResult(string corporationId, string accountItemName)
        {
            var (unit, consolidatedAccountValues) = await ReadUnitAndConsolidatedAccountValues(connection, corporationId, accountItemName);
            var (_, nonConsolidatedAccountValues) = await ReadUnitAndNonConsolidatedAccountValues(connection, corporationId, accountItemName);
            return new TimeSeriesAnalysisResult
            {
                AccountName = accountItemName,
                Unit = unit,
                ConsolidatedValues = consolidatedAccountValues,
                NonConsolidatedValues = nonConsolidatedAccountValues,
                Corporation = await corporationRepository.Get(corporationId)
                    ?? throw new ArgumentException("指定された企業は存在しません")
            };
        }

        private static IUnit GetUnit(NpgsqlDataReader reader)
        {
            var measureColumn = reader.GetOrdinal("measure");
            var unitNameColumn = reader.GetOrdinal("unit_name");
            var measure = reader[measureColumn];
            if (measure is DBNull)
            {
                var unitNumeratorColumn = reader.GetOrdinal("unit_numerator");
                var unitDenominatorColumn = reader.GetOrdinal("unit_denominator");
                return new DividedUnit
                {
                    Name = $"{reader[unitNameColumn]}",
                    UnitNumerator = $"{reader[unitNumeratorColumn]}",
                    UnitDenominator = $"{reader[unitDenominatorColumn]}"
                };
            }
            return new NormalUnit
            {
                Name = $"{reader[unitNameColumn]}",
                Measure = $"{reader[measureColumn]}"
            };
        }
        private static async Task<(IUnit?, IReadOnlyList<AccountValue>)> ReadUnitAndConsolidatedAccountValues(NpgsqlConnection connection, string corporationId, string accountItemName)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    A.amounts,
    A.unit_name,
    E.measure,
    E.unit_numerator,
    E.unit_denominator,
    C.context_name,
    C.period_from,
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
INNER JOIN
    units E
ON
    A.unit_name = E.unit_name
AND
    A.report_id = E.report_id
WHERE
    B.account_name = @accountName
AND
    C.context_name IN ('CurrentYearInstant', 'CurrentYearDuration')
AND
    D.code = @corporationId
GROUP BY
    A.amounts,
    A.unit_name,
    E.measure,
    E.unit_numerator,
    E.unit_denominator,
    C.context_name,
    C.period_from,
    C.period_to,
    C.instant_date
ORDER BY
    period_from, instant_date;
";
            command.Parameters.Add("@accountName", NpgsqlDbType.Varchar)
                .Value = accountItemName;
            command.Parameters.Add("@corporationId", NpgsqlDbType.Varchar)
                .Value = corporationId;
            using var reader = await command.ExecuteReaderAsync();
            return await GetAccountValues(reader);
        }
        private static async Task<(IUnit?, IReadOnlyList<AccountValue>)> GetAccountValues(NpgsqlDataReader reader)
        {
            var values = new List<AccountValue>();
            var amountsColumn = reader.GetOrdinal("amounts");
            var instantDateColumn = reader.GetOrdinal("instant_date");
            var fromDateColumn = reader.GetOrdinal("period_from");
            var toDateColumn = reader.GetOrdinal("period_to");
            IUnit? unit = null;
            while (await reader.ReadAsync())
            {
                unit ??= GetUnit(reader);
                values.Add(new AccountValue
                {
                    FinancialAccountPeriod = GetAccountsPeriod(reader, instantDateColumn, fromDateColumn, toDateColumn),
                    Amount = decimal.Parse($"{reader[amountsColumn]}")
                });
            }
            return (unit, values);
        }

        private static async Task<(IUnit?, IReadOnlyList<AccountValue>)> ReadUnitAndNonConsolidatedAccountValues(NpgsqlConnection connection, string corporationId, string accountItemName)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    A.amounts,
    A.unit_name,
    E.measure,
    E.unit_numerator,
    E.unit_denominator,
    C.context_name,
    C.period_from,
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
INNER JOIN
    units E
ON
    A.unit_name = E.unit_name
AND
    A.report_id = E.report_id
WHERE
    B.account_name = @accountName
AND
    C.context_name IN ('CurrentYearInstant_NonConsolidatedMember', 'CurrentYearDuration_NonConsolidatedMember')
AND
    D.code = @corporationId
GROUP BY
    A.amounts,
    A.unit_name,
    E.measure,
    E.unit_numerator,
    E.unit_denominator,
    C.context_name,
    C.period_from,
    C.period_to,
    C.instant_date
ORDER BY
    period_from, instant_date;
";
            command.Parameters.Add("@accountName", NpgsqlDbType.Varchar)
                .Value = accountItemName;
            command.Parameters.Add("@corporationId", NpgsqlDbType.Varchar)
                .Value = corporationId;
            using var reader = await command.ExecuteReaderAsync();
            return await GetAccountValues(reader);
        }
        private static IAccountsPeriod GetAccountsPeriod(NpgsqlDataReader reader, int instantDateColumn, int fromDateColumn, int toDateColumn)
        {
            if (reader[instantDateColumn] is DBNull)
            {
                return new DurationPeriod
                {
                    From = DateTime.Parse($"{reader[fromDateColumn]}"),
                    To = DateTime.Parse($"{reader[toDateColumn]}")
                };
            }
            return new InstantPeriod
            {
                Instant = DateTime.Parse($"{reader[instantDateColumn]}")
            };
        }
    }
}
