using System;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.TimeSeriesAnalysis.Corporations
{
    public sealed class CorporationRepository : ICorporationRepository, IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlConnection connection;

        public CorporationRepository()
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

        public async Task<Corporation?> Get(string corporationId)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    is_linking,
    capital_amount,
    submitter_name,
    type_of_industry
FROM
    company_master
WHERE
    code = @corporationId
LIMIT 1
";
            command.Parameters.Add("@corporationId", NpgsqlDbType.Varchar)
                .Value = corporationId;
            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }
            return new Corporation
            {
                IsLinking = bool.Parse($"{reader["is_linking"]}"),
                CapitalAmount = decimal.Parse($"{reader["capital_amount"]}"),
                Name = $"{reader["submitter_name"]}",
                TypeOfIndustry = $"{reader["type_of_industry"]}"
            };
        }
    }
}
