using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using PostgreSQLCopyHelper;
using ResearchXBRL.CrossCuttingInterest.Extensions;
using ResearchXBRL.Domain.AccountItemReverseLookup.AccountItems;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.AccountItemReverseLookup.AccountItems;

public sealed class AccountItemsRepository : SQLService, IAccountItemRepository
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
        var tmpTableName = $"tmp_aggregation_of_names_list_{Guid.NewGuid()}";
        command.CommandText = $@"
CREATE TABLE TEMP {tmpTableName} (
  aggregate_target VARCHAR PRIMARY KEY,
  aggregate_result VARCHAR NOT NULL,
);
        ";
        await command.ExecuteNonQueryAsync();
        return tmpTableName;
    }
    private static async ValueTask BulkInsert(IEnumerable<AccountItem> accountItems, NpgsqlConnection connection, string tmpTableName)
    {
        var reportCoverHelper = new PostgreSQLCopyHelper<AccountItem>(tmpTableName)
            .MapVarchar("aggregate_target", x => x.NormalizedName)
            .MapVarchar("aggregate_result", x => x.OriginalName);
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
    99
FROM
    {tmpTableName} TMP
LEFT OUTER JOIN
    aggregation_of_names_list TARGET
ON
    TARGET.aggregate_target = TMP.aggregate_target
AND
    TARGET.aggregate_result = TMP.aggregate_result
WHERE
    TARGET.aggregate_target IS NULL;
";
        await command.ExecuteNonQueryAsync();
    }
}
