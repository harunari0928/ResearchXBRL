using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItemMenus;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public sealed class AccountItemMenuRepository : IAccountItemMenuRepository, IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlConnection connection;

        public AccountItemMenuRepository()
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

        public async Task<AccountItemMenu> GetProposals(string keyword)
        {
            return new AccountItemMenu
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
    account_name = '売上高'
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
}
