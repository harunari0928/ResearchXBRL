using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.CorporationMenus;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.AnalysisMenus.CorporationMenus
{
    public sealed class CorporationMenuRepository : ICorporationMenuRepository, IDisposable, IAsyncDisposable
    {
        private readonly NpgsqlConnection connection;

        public CorporationMenuRepository()
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

        public async Task<CorporatonMenu> GetProposals(string keyword)
        {
            var command = CreateReadCommand(keyword);
            return new CorporatonMenu
            {
                Corporations = await ReadCorporations(command)
            };
        }

        private NpgsqlCommand CreateReadCommand(string keyword)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
SELECT
    code,
    submitter_name
FROM
    company_master
WHERE
    submission_type LIKE '%法人%' -- 法人のみをサジェストする
AND (
    submitter_name LIKE @submitterName
OR
    submitter_name_yomigana LIKE @submitterNameKana
)
LIMIT 10;
";
            command.Parameters.Add("@submitterName", NpgsqlDbType.Varchar)
                .Value = $"%{keyword}%";
            command.Parameters.Add("@submitterNameKana", NpgsqlDbType.Varchar)
                .Value = $"%{ToKatakana(keyword)}%";
            return command;
        }
        private static async Task<IReadOnlyList<Corporation>> ReadCorporations(NpgsqlCommand command)
        {
            var reader = await command.ExecuteReaderAsync();
            var corporations = new List<Corporation>();
            while (await reader.ReadAsync())
            {
                if (reader[0] is null || reader[1] is null)
                {
                    continue;
                }

                corporations.Add(new Corporation
                {
                    Name = reader[1]?.ToString() ??
                        throw new NullReferenceException(),
                    CorporationId = reader[0]?.ToString() ??
                        throw new NullReferenceException()
                });
            }

            return corporations;
        }
        private static string ToKatakana(string str)
        {
            return string.Concat(str.Select(c => (c >= 'ぁ' && c <= 'ゖ') ? (char)(c + 'ァ' - 'ぁ') : c));
        }
    }
}
