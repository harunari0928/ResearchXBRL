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
    public async ValueTask<IReadOnlyCollection<ReverseLookupResult>> Lookup(FinancialReport financialReport)
    {
        if (financialReport.NetSales is null)
        {
            return Enumerable.Empty<ReverseLookupResult>().ToArray();
        }

        return await GetXBRLNames(financialReport)
            .Select(x => new ReverseLookupResult("NetSales", x))
            .ToArrayAsync();
    }

    private async IAsyncEnumerable<string> GetXBRLNames(FinancialReport financialReport)
    {
        using var command = connection.CreateCommand();
        SetSQLQuery(financialReport, command);
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
            yield return value;
        }
    }
    private static void SetSQLQuery(FinancialReport financialReport, Npgsql.NpgsqlCommand command)
    {
        command.CommandText = @"
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
AND
    C.context_name IN ('CurrentYearInstant', 'CurrentYearDuration')
AND
    D.securities_code = @securitiesCode
AND
    (C.period_to = @fiscalYear OR C.instant_date = @fiscalYear)
GROUP BY
    A.xbrl_name
";
        command.Parameters.Add("@amounts", NpgsqlDbType.Numeric)
            .Value = financialReport.NetSales * 1000000;
        command.Parameters.Add("@securitiesCode", NpgsqlDbType.Varchar)
            .Value = $"{financialReport.SecuritiesCode}0";
        command.Parameters.Add("@fiscalYear", NpgsqlDbType.Date)
            .Value = financialReport.FiscalYear.ToDateTime(TimeOnly.MinValue);
    }
}
