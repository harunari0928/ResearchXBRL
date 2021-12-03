using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using PostgreSQLCopyHelper;
using ResearchXBRL.Domain.AccountElements;

namespace ResearchXBRL.Infrastructure.AccountElements
{
    public sealed class AccountElementRepository : IAccountElementRepository, IAsyncDisposable
    {
        private readonly NpgsqlConnection connection;

        public AccountElementRepository()
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

        public async Task Write(IEnumerable<AccountElement> elements)
        {
            using var tran = connection.BeginTransaction();
            var helper = new PostgreSQLCopyHelper<AccountElement>("account_elements")
                .MapVarchar("xbrl_name", x => x.XBRLName)
                .MapDate("taxonomy_version", x => x.TaxonomyVersion)
                .MapVarchar("account_name", x => x.AccountName)
                .MapVarchar("classification", x => x.Classification);
            await helper.SaveAllAsync(connection, elements);
            await tran.CommitAsync();
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
        }

        public async Task Clean()
        {
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM account_elements";
            await command.ExecuteNonQueryAsync();
        }
    }
}
