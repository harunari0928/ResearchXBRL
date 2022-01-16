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
            var command = CreateReadCommand(keyword);
            return new AccountItemMenu
            {
                AccountItems = await ReadAccountItems(command).ToListAsync()
            };
        }

        private NpgsqlCommand CreateReadCommand(string keyword)
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
    account_name LIKE @accountName
GROUP BY
    account_name
LIMIT 10;
";
            command.Parameters.Add("@accountName", NpgsqlDbType.Varchar)
                .Value = $"%{keyword}%";
            return command;
        }

        private static async IAsyncEnumerable<AccountItem> ReadAccountItems(NpgsqlCommand command)
        {
            var reader = await command.ExecuteReaderAsync();
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
