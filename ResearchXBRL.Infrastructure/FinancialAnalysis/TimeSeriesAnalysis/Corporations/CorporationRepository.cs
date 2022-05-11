using System;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ResearchXBRL.Domain.FinancialAnalysis.TimeSeriesAnalysis.Corporations;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.FinancialAnalysis.TimeSeriesAnalysis.Corporations;

public sealed class CorporationRepository : ThreadUnsafeSQLService, ICorporationsRepository
{
    public async ValueTask<bool> Exists(string corporationId)
    {
        if (string.IsNullOrWhiteSpace(corporationId))
        {
            return false;
        }

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
        return await reader.ReadAsync();
    }
}
