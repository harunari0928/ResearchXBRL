using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using PostgreSQLCopyHelper;
using ResearchXBRL.CrossCuttingInterest.Extensions;
using ResearchXBRL.Domain.ReverseLookupAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.ReverseLookupAccountItems.AccountItems;

public sealed class AccountItemsRepository : SQLService, IAccountItemsRepository
{
    public async ValueTask Add(IAsyncEnumerable<AccountItem> normalizedAccountItems)
    {
        await foreach (var chunkedAccountItems in normalizedAccountItems.Chunk(5000))
        {
            await InsertAccountItems(chunkedAccountItems);
        }
    }

    private async ValueTask InsertAccountItems(IEnumerable<AccountItem> accountItems)
    {
        using var tran = connection.BeginTransaction();
        var tmpTableName = await CreateTmpTable(connection);
        await BulkInsert(accountItems, connection, tmpTableName);
        await tran.CommitAsync();
    }
    private static async ValueTask<string> CreateTmpTable(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();
        var tmpTableName = $"tmp_aggregation_of_names_list_{Guid.NewGuid():N}";
        command.CommandText = $@"
CREATE TEMP TABLE {tmpTableName} (
  aggregate_target VARCHAR NOT NULL,
  aggregate_result VARCHAR NOT NULL
);
        ";
        await command.ExecuteNonQueryAsync();
        return tmpTableName;
    }
    private static async ValueTask BulkInsert(IEnumerable<AccountItem> accountItems, NpgsqlConnection connection, string tmpTableName)
    {
        var reportCoverHelper = new PostgreSQLCopyHelper<AccountItem>(tmpTableName)
            .MapVarchar("aggregate_target", x => x.OriginalName)
            .MapVarchar("aggregate_result", x => x.NormalizedName);
        await reportCoverHelper.SaveAllAsync(connection, accountItems);
        await InsertIntoPersistenceTable(connection, tmpTableName);
    }
    private static async ValueTask InsertIntoPersistenceTable(NpgsqlConnection connection, string tmpTableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
INSERT INTO aggregation_of_names_list
SELECT
    TMP.aggregate_target,
    TMP.aggregate_result,
    9999999
FROM
    {tmpTableName} TMP
LEFT OUTER JOIN
    aggregation_of_names_list TARGET
ON
    TARGET.aggregate_target = TMP.aggregate_target
AND
    TARGET.aggregate_result = TMP.aggregate_result
WHERE
    TARGET.aggregate_target IS NULL
AND
    TMP.aggregate_target <> TMP.aggregate_result -- (NetSales, NetSales)といったデータを除く
GROUP BY
    TMP.aggregate_target, TMP.aggregate_result;
";
        await command.ExecuteNonQueryAsync();
    }
}
