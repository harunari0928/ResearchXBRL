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
            var (amounts, priority) = financialReport.AccountAmounts[accountName];
            if (amounts is null)
            {
                continue;
            }

            SetSQLQuery(accountName, (decimal)amounts, financialReport.SecuritiesCode, financialReport.FiscalYear, command);
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

                if (financialReport.AccountAmounts.ContainsKey(value))
                {
                    continue;
                }

                yield return new(accountName, value, priority);
            }
        }
    }
    private static void SetSQLQuery(string accountName, decimal amounts, decimal securitiesCode, DateOnly fiscalYear, Npgsql.NpgsqlCommand command)
    {
        if (accountName == "DividendPaidPerShareSummaryOfBusinessResults")
        {
            command.CommandText = GetDividendQueryString(command);
        }
        else if (IsDurationPeriod(accountName))
        {
            command.CommandText = GetDurationPeriodQueryString(command);
        }
        else
        {
            command.CommandText = GetInstantPeriodQueryString(command);
        }
        SetSQLQueryParameters(
            amounts,
            securitiesCode,
            fiscalYear, command);
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
    private static string GetDividendQueryString(Npgsql.NpgsqlCommand command) => @"
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
WHERE
    A.amounts = @amounts
AND
    C.context_name = 'CurrentYearDuration_NonConsolidatedMember'
AND
    D.securities_code = @securitiesCode
AND
    C.period_to = @fiscalYear
GROUP BY
    A.xbrl_name;
";
    private static string GetDurationPeriodQueryString(Npgsql.NpgsqlCommand command) => @"
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
WHERE
    A.amounts = @amounts
AND
    C.context_name = 'CurrentYearDuration'
AND
    D.securities_code = @securitiesCode
AND
    C.period_to = @fiscalYear
GROUP BY
    A.xbrl_name;
";
    private static string GetInstantPeriodQueryString(Npgsql.NpgsqlCommand command) => @"
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
WHERE
    A.amounts = @amounts
AND
    C.context_name = 'CurrentYearInstant'
AND
    D.securities_code = @securitiesCode
AND
    C.instant_date = @fiscalYear
GROUP BY
    A.xbrl_name;
";
    private static bool IsDurationPeriod(in string accountName)
    {
        switch (accountName)
        {
            case "NetSales":
            case "OrdinaryIncome":
            case "OperatingIncome":
            case "ProfitLossAttributableToOwnersOfParent":
            case "GrossProfit":
            case "NetCashProvidedByUsedInOperatingActivities":
                return true;
            case "TotalAssets":
            case "NetAssets":
            case "Liabilities":
                return false;
            default:
                throw new NotSupportedException();
        }
    }
}