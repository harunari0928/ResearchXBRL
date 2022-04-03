using System.Collections.Generic;
using System.Threading.Tasks;
using PostgreSQLCopyHelper;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.Shared;

namespace ResearchXBRL.Infrastructure.ImportAccountItems.AccountItems;

public sealed class AccountItemsRepository : SQLService, IAccountItemsRepository
{
    public async Task Write(IEnumerable<AccountItem> elements)
    {
        using var tran = connection.BeginTransaction();
        var helper = new PostgreSQLCopyHelper<AccountItem>("account_elements")
            .MapVarchar("xbrl_name", x => x.XBRLName)
            .MapDate("taxonomy_version", x => x.TaxonomyVersion)
            .MapVarchar("account_name", x => x.AccountName)
            .MapVarchar("classification", x => x.Classification);
        await helper.SaveAllAsync(connection, elements);
        await tran.CommitAsync();
    }

    public async Task Clean()
    {
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM account_elements";
        await command.ExecuteNonQueryAsync();
    }
}
