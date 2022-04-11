using System;
using System.Collections.Generic;
using System.Globalization;
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

    public AccountItemsRepository(IFileStorage fileStorage, string outputFilePath)
    {
        fileStorage.Delete(outputFilePath);
        csvWriter = new CsvWriter(fileStorage.CreateFile(outputFilePath), CultureInfo.CurrentCulture);
    }

    public async ValueTask Add(IAsyncEnumerable<AccountItem> normalizedAccountItems)
    {
        csvWriter.WriteHeader<AccountItemInCsv>();
        await csvWriter.NextRecordAsync();
        await csvWriter.FlushAsync();
        var writeHistory = new HashSet<string>();
        await foreach (var chunkedAccountItems in normalizedAccountItems.Chunk(5000))
        {
            var distincted = chunkedAccountItems
                .Select(x => new AccountItemInCsv(x))
                .Where(x => !writeHistory.Contains($"{x.NormalizedName}_{x.OriginalName}"))
                .GroupBy(x => $"{x.NormalizedName}_{x.OriginalName}")
                .Select(x => x.First())
                .ToArray();
            await csvWriter.WriteRecordsAsync(distincted);
            await csvWriter.FlushAsync();
            foreach (var x in distincted)
            {
                writeHistory.Add($"{x.NormalizedName}_{x.OriginalName}");
            }
        }
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
