using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using ResearchXBRL.CrossCuttingInterest.Extensions;
using ResearchXBRL.Domain.ReverseLookupAccountItems.AccountItems;
using ResearchXBRL.Infrastructure.Shared.FileStorages;

namespace ResearchXBRL.Infrastructure.ReverseLookupAccountItems.AccountItems;

public sealed class AccountItemsRepository : IAccountItemsRepository, IAsyncDisposable, IDisposable
{
    private readonly CsvWriter csvWriter;
    private readonly IFileStorage fileStorage;
    private readonly Stream memoryStream = new MemoryStream();
    private readonly string outputFilePath;

    public AccountItemsRepository(IFileStorage fileStorage, string outputFilePath)
    {
        var writer = new StreamWriter(memoryStream);
        csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);
        this.fileStorage = fileStorage;
        this.outputFilePath = outputFilePath;
    }

    public async ValueTask Add(IAsyncEnumerable<AccountItem> normalizedAccountItems)
    {
        csvWriter.WriteHeader<AccountItemInCsv>();
        await csvWriter.NextRecordAsync();
        var writeHistory = new HashSet<(string normalizedName, string originalName)>();
        await foreach (var chunkedAccountItems in normalizedAccountItems.Chunk(5000))
        {
            await csvWriter.WriteRecordsAsync(chunkedAccountItems.Select(x => new AccountItemInCsv(x))
                .Where(x => !writeHistory.Contains((x.NormalizedName, x.OriginalName))));
            foreach (var item in chunkedAccountItems)
            {
                writeHistory.Add((item.NormalizedName, item.OriginalName));
            }
        }
        fileStorage.Set(memoryStream, outputFilePath);
    }

    public void Dispose()
    {
        csvWriter.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await csvWriter.DisposeAsync();
    }

    private sealed class AccountItemInCsv
    {
        [Name("名寄せ先")]
        public string NormalizedName { get; }

        [Name("XBRL名")]
        public string OriginalName { get; }

        [Name("証券コード")]
        public int SecuritiesCode { get; }

        [Name("会計年度")]
        [Format("yyyy-MM-dd")]
        public DateTime FiscalYear { get; }

        public AccountItemInCsv(AccountItem accountItem)
        {
            NormalizedName = accountItem.NormalizedName;
            OriginalName = accountItem.OriginalName;
            SecuritiesCode = accountItem.SecuritiesCode;
            FiscalYear = accountItem.FiscalYear.ToDateTime(TimeOnly.MinValue);
        }
    }
}
