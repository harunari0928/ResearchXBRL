using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItems;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.AnalysisMenus.AccountItems;

public sealed class AccountItemsMenuRepository : SQLService, IAccountItemsMenuRepository
{
    public async Task<AccountItemsMenu> GetProposals(string keyword)
    {
        return new AccountItemsMenu
        {
            SearchedAccountItem = await ReadSearchedAccountItems(keyword),
            SuggestedAccountItems = await ReadSuggestedAccountItems(keyword).ToListAsync()
        };
    }

    private NpgsqlCommand CreateReadSearchedCommand(string keyword)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    account_name
FROM
    account_elements
WHERE
    classification IN ('jppfs', 'jpigp')
AND
    account_name = @searchedAccountName
LIMIT 1;
";
        command.Parameters.Add("@searchedAccountName", NpgsqlDbType.Varchar)
            .Value = keyword;
        return command;
    }

    private NpgsqlCommand CreateReadSuggestedCommand(string keyword)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
    account_name
FROM
    account_elements
WHERE
    classification IN ('jppfs', 'jpigp')
AND
    account_name <> @searchedAccountName
AND
    account_name LIKE @likeSearchedAccountName
GROUP BY
    account_name
LIMIT 10;
";
        command.Parameters.Add("@searchedAccountName", NpgsqlDbType.Varchar)
            .Value = keyword;
        command.Parameters.Add("@likeSearchedAccountName", NpgsqlDbType.Varchar)
            .Value = $"%{keyword}%";
        return command;
    }

    private async Task<AccountItem?> ReadSearchedAccountItems(string keyword)
    {
        using var command = CreateReadSearchedCommand(keyword);
        var accountName = await command.ExecuteScalarAsync() as string;
        if (accountName is null)
        {
            return null;
        }

        return new AccountItem
        {
            Name = accountName
        };
    }

    private async IAsyncEnumerable<AccountItem> ReadSuggestedAccountItems(string keyword)
    {
        using var command = CreateReadSuggestedCommand(keyword);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var accountName = reader[reader.GetOrdinal("account_name")].ToString();
            if (accountName is null)
            {
                continue;
            }

            yield return new AccountItem { Name = accountName };
        }
    }
}
