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
        foreach (var accountName in financialReport.AccountAmounts.Keys)
        {
            var amounts = financialReport.AccountAmounts[accountName];
            if (amounts is null)
            {
                continue;
            }

            using var command = connection.CreateCommand();
            SetSQLQuery(accountName, (decimal)amounts, financialReport.SecuritiesCode, financialReport.FiscalYear, command);
            using var reader = await command.ExecuteReaderAsync();
            var xbrlNameIndex = reader.GetOrdinal("xbrl_name");
            while (await reader.ReadAsync())
            {
                if (reader[xbrlNameIndex] is DBNull)
                {
                    continue;
                }

                var xbrlName = reader[xbrlNameIndex]?.ToString() ?? "";
                if (string.IsNullOrEmpty(xbrlName))
                {
                    continue;
                }

                if (financialReport.AccountAmounts.ContainsKey(xbrlName))
                {
                    continue;
                }

                yield return new(accountName, xbrlName, financialReport.SecuritiesCode, financialReport.FiscalYear);
            }
        }
    }
    private static void SetSQLQuery(string accountName, decimal amounts, decimal securitiesCode, DateOnly fiscalYear, Npgsql.NpgsqlCommand command)
    {
        if (accountName == "配当金")
        {
            command.CommandText = GetDividendQueryString(command);
        }
        else if (accountName == "営業活動によるキャッシュフロー")
        {
            command.CommandText = GetCashFlowQueryString(command);
        }
        else if (IsPLAccount(accountName))
        {
            command.CommandText = GetPLQueryString(command);
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
    private static string GetPLQueryString(Npgsql.NpgsqlCommand command) => @"
SELECT
    A.xbrl_name
FROM
    report_items A
LEFT OUTER JOIN
    account_elements B
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
WHERE
    A.amounts = @amounts
AND
    C.context_name = 'CurrentYearDuration'
AND
    D.securities_code = @securitiesCode
AND
    C.period_to = @fiscalYear
AND
    A.xbrl_name NOT LIKE '%Comprehensive%'
AND
    (B.balance = 'credit' OR B.balance IS NULL) -- account_elementsテーブルに情報がない勘定科目はとりあえず候補にあげる
GROUP BY
    A.xbrl_name;
";
    private static string GetCashFlowQueryString(Npgsql.NpgsqlCommand command) => @"
SELECT
    A.xbrl_name
FROM
    report_items A
INNER JOIN
    account_elements B
ON
    A.xbrl_name = B.xbrl_name
AND
    B.balance = ''
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
AND
    A.xbrl_name NOT LIKE '%Comprehensive%'
GROUP BY
    A.xbrl_name;
";
    private static string GetInstantPeriodQueryString(Npgsql.NpgsqlCommand command) => @"
SELECT
    A.xbrl_name
FROM
    report_items A
LEFT OUTER JOIN
    account_elements B
ON
    A.xbrl_name = B.xbrl_name
AND
    B.balance = 'credit'
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
AND
    (B.balance = 'credit' OR B.balance IS NULL) -- account_elementsテーブルに情報がない勘定科目はとりあえず候補にあげる
GROUP BY
    A.xbrl_name;
";
    private static bool IsPLAccount(in string accountName)
    {
        switch (accountName)
        {
            case "売上高":
            case "経常利益":
            case "営業利益":
            case "親会社帰属利益":
            case "売上総利益":
                return true;
            case "総資産":
            case "純資産":
            case "総負債":
            case "営業活動によるキャッシュフロー":
                return false;
            default:
                throw new NotSupportedException();
        }
    }
}