using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.Corporations;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.AnalysisMenus.Corporations;

public sealed class CorporationsMenuRepository : ThreadUnsafeSQLService, ICorporationsMenuRepository
{
    public async Task<CorporatonsMenu> GetProposals(string keyword)
    {
        using var command = CreateReadCommand(keyword);
        return new CorporatonsMenu
        {
            Corporations = await ReadCorporations(command)
        };
    }

    public async ValueTask<Corporation?> FindBySecuritiesCode(string securitiesCode)
    {
        using var command = CreateReadCommandBySecuritiesCode(securitiesCode);
        var suggestedCorporations = await ReadCorporations(command);
        return suggestedCorporations.FirstOrDefault();
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
    private NpgsqlCommand CreateReadCommandBySecuritiesCode(string securitiesCode)
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
AND
    securities_code = @securitiesCode
LIMIT 1;
";
        command.Parameters.Add("securitiesCode", NpgsqlDbType.Varchar)
            .Value = $"{securitiesCode}0";
        return command;
    }
    private static async Task<IReadOnlyList<Corporation>> ReadCorporations(NpgsqlCommand command)
    {
        using var reader = await command.ExecuteReaderAsync();
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
