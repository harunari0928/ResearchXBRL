using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NpgsqlTypes;
using ResearchXBRL.Application.DTO.ReverseLookupAccountItems;
using ResearchXBRL.Application.QueryServices.ReverseLookupAccountItems;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.QueryServices.ReverseLookupAccountItems;

public sealed class ReverseLookupQueryService : SQLService, IReverseLookupQueryService
{
    public async ValueTask<IReadOnlyCollection<ReverseLookupResult>> Lookup(FinancialReport financialReport) =>
        await GetNetSalesXBRLNames(financialReport)
            .ToArrayAsync();

    private async IAsyncEnumerable<ReverseLookupResult> GetNetSalesXBRLNames(FinancialReport financialReport)
    {
        using var command = connection.CreateCommand();
        foreach (var accountName in financialReport.AccountAmounts.Keys)
        {
            SetSQLQuery(accountName, financialReport, command);
            using var reader = await command.ExecuteReaderAsync();
            var xbrlNameIndex = reader.GetOrdinal("xbrl_name");
            while (await reader.ReadAsync())
            {
                if (reader[xbrlNameIndex] is DBNull)
                {
                    continue;
                }

                var value = reader[xbrlNameIndex]?.ToString() ?? "";
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }
                yield return new(accountName, value);
            }
        }
    }
    private static void SetSQLQuery(string accountName, FinancialReport financialReport, Npgsql.NpgsqlCommand command)
    {
        command.CommandText = GetSQLQueryString(command);
        SetSQLQueryParameters(
            financialReport.AccountAmounts[accountName],
            financialReport.SecuritiesCode,
            financialReport.FiscalYear, command);
    }
    private static void SetSQLQueryParameters(decimal amount, decimal securitiesCode, DateOnly fiscalYear, Npgsql.NpgsqlCommand command)
    {
        command.Parameters.Add("@amounts", NpgsqlDbType.Numeric)
            .Value = amount;
        command.Parameters.Add("@securitiesCode", NpgsqlDbType.Varchar)
            .Value = $"{securitiesCode}0";
        command.Parameters.Add("@fiscalYear", NpgsqlDbType.Date)
            .Value = fiscalYear.ToDateTime(TimeOnly.MinValue);
    }
    private static string GetSQLQueryString(Npgsql.NpgsqlCommand command) => @"
SELECT
    A.xbrl_name
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
INNER JOIN
    units E
ON
    A.unit_name = E.unit_name
AND
    A.report_id = E.report_id
WHERE
    A.amounts = @amounts
ND
    C.context_name IN ('CurrentYearInstant_NonConsolidatedMember', 'CurrentYearDuration_NonConsolidatedMember', 'CurrentYearInstant', 'CurrentYearDuration')
AND
    D.securities_code = @securitiesCode
AND
    (C.period_to = @fiscalYear OR C.instant_date = @fiscalYear)
GROUP BY
    A.xbrl_name
";
}
