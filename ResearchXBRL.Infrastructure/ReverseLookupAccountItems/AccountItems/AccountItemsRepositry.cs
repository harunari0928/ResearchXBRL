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
  aggregate_result VARCHAR NOT NULL,
  priority INT NOT NULL
);
        ";
        await command.ExecuteNonQueryAsync();
        return tmpTableName;
    }
    private static async ValueTask BulkInsert(IEnumerable<AccountItem> accountItems, NpgsqlConnection connection, string tmpTableName)
    {
        var reportCoverHelper = new PostgreSQLCopyHelper<AccountItem>(tmpTableName)
            .MapVarchar("aggregate_target", x => x.OriginalName)
            .MapVarchar("aggregate_result", x => x.NormalizedName)
            .MapInteger("priority", x => x.Priority);
        await reportCoverHelper.SaveAllAsync(connection, accountItems);
        await InsertIntoPersistenceTable(connection, tmpTableName);
    }
    private static async ValueTask InsertIntoPersistenceTable(NpgsqlConnection connection, string tmpTableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
INSERT INTO aggregation_of_names_list
SELECT 
    A.aggregate_target,
    A.aggregate_result,
    9999999
FROM (
    SELECT
        TMP.aggregate_target,
        TMP.aggregate_result,
        TMP.priority,
        MIN(TMP.priority) OVER (PARTITION BY TMP.aggregate_target) AS highest_priority
    FROM
        {tmpTableName} TMP
    LEFT OUTER JOIN
        aggregation_of_names_list TARGET
    ON
        TARGET.aggregate_target = TMP.aggregate_target
    WHERE
        TARGET.aggregate_target IS NULL
    AND
        TMP.aggregate_target <> TMP.aggregate_result -- (NetSales, NetSales)といったデータを除く
    ) A
WHERE
    A.priority = A.highest_priority
GROUP BY
    A.aggregate_target, A.aggregate_result;
";
        await command.ExecuteNonQueryAsync();
    }
}
