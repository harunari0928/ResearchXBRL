using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;

namespace ResearchXBRL.Infrastructure.ImportAccountItems.AccountItems;

public sealed class AccountItemsCSVWriter : IAccountItemsWriter
{
    private readonly CsvWriter csvWriter;

    public AccountItemsCSVWriter(TextWriter writer)
    {
        csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);
    }

    public async Task Write(IEnumerable<AccountItem> elements)
    {
        csvWriter.WriteHeader<AccountItem>();
        await csvWriter.NextRecordAsync();
        await csvWriter.WriteRecordsAsync(elements);
    }

    public void Dispose()
    {
        csvWriter.Dispose();
    }
}
