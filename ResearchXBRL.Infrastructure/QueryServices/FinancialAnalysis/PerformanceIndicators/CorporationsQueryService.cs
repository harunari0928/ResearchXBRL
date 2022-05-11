using System.Threading.Tasks;
using NpgsqlTypes;
using ResearchXBRL.Application.QueryServices.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.QueryServices.FinancialAnalysis.PerformanceIndicators;

public class CorporationsQueryService : ThreadUnsafeSQLService, ICorporationsQueryService
{
    public async ValueTask<bool> Exists(string corporationId)
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
        return await reader.ReadAsync();
    }
}
