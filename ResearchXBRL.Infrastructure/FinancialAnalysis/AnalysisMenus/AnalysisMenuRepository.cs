using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Npgsql;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.AnalysisMenus
{
    public sealed class AnalysisMenuRepository : IAnalysisMenuRepository
    {
        private readonly NpgsqlConnection connection;

        public AnalysisMenuRepository()
        {
            var server = Environment.GetEnvironmentVariable("DB_SERVERNAME");
            var userId = Environment.GetEnvironmentVariable("DB_USERID");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var connectionString = $"Server={server};Port={port};Database={dbName};User Id={userId};Password={password};";
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

        public async Task<AnalysisMenu> Get()
        {
            return new AnalysisMenu
            {
                AccountItems = await GetAccountItems(),
                Corporations = await GetCorporations()
            };
        }

        private async Task<IReadOnlyList<AccountItem>> GetAccountItems()
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    account_name
FROM
    account_elements
GROUP BY
    account_name
";
            using var reader = await command.ExecuteReaderAsync();
            var accountItems = new List<AccountItem>();
            while (await reader.ReadAsync())
            {
                var accountName = reader[0]?.ToString();

                if (accountName is null) { continue; }

                accountItems.Add(new AccountItem
                {
                    Name = accountName
                });
            }
            return accountItems;
        }
        private async Task<IReadOnlyList<Corporation>> GetCorporations()
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    submitter_name
FROM
    company_master
";
            using var reader = await command.ExecuteReaderAsync();
            var accountItems = new List<Corporation>();
            while (await reader.ReadAsync())
            {
                var accountName = reader[0]?.ToString();

                if (accountName is null) { continue; }

                accountItems.Add(new Corporation
                {
                    Name = accountName
                });
            }
            return accountItems;
        }
    }
}
