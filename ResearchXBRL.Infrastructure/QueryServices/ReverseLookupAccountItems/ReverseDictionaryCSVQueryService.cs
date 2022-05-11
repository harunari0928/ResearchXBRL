using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using ResearchXBRL.Application.DTO.Results;
using ResearchXBRL.Application.DTO.ReverseLookupAccountItems;
using ResearchXBRL.Application.QueryServices.ReverseLookupAccountItems;
using ResearchXBRL.Infrastructure.Shared.FileStorages;

namespace ResearchXBRL.Infrastructure.QueryServices.ReverseLookupAccountItems;

public sealed class ReverseDictionaryCSVQueryService : IReverseDictionaryQueryService
{
    private readonly IFileStorage fileStorage;
    private readonly string filePath;

    public ReverseDictionaryCSVQueryService(IFileStorage fileStorage, string filePath)
    {
        this.fileStorage = fileStorage;
        this.filePath = filePath;
    }

    public IResult<IAsyncEnumerable<FinancialReport>> Get()
    {
        var fileStream = fileStorage.Get(filePath);
        if (fileStream is null)
        {
            return new Abort<IAsyncEnumerable<FinancialReport>>
            {
                Message = $"対象のファイルが存在しません: {filePath}"
            };
        }

        var streamReader = new StreamReader(fileStream);
        var reader = new CsvReader(streamReader, CultureInfo.CurrentCulture, true);
        return new Succeeded<IAsyncEnumerable<FinancialReport>>(ReadFinancialReports(reader,
            new IDisposable[] { reader, streamReader, fileStream }));
    }

    private static async IAsyncEnumerable<FinancialReport> ReadFinancialReports(CsvReader reader, IEnumerable<IDisposable> resources)
    {
        try
        {
            await foreach (IDictionary<string, object> record in reader.GetRecordsAsync<dynamic>())
            {
                var accountingStandard = (AccountingStandards)int.Parse(record["会計基準"]?.ToString()
                    ?? throw new Exception($"会計基準が不正です: {record["会計基準"]}"));

                if (record["証券コード"]?.ToString() == "NA")
                {
                    continue; // 未上場企業は無視
                }

                var accountAmounts = GetAccountAmounts(record);

                yield return new FinancialReport
                {
                    SecuritiesCode = ParseInt(record["証券コード"]?.ToString(), "証券コード"),
                    AccountingStandard = accountingStandard,
                    FiscalYear = DateOnly.Parse(record["会計年度"]?.ToString() ?? throw new Exception($"会計年度が不正です: {record["会計年度"]}")),
                    AccountAmounts = accountAmounts
                };
            }
        }
        finally
        {
            DisposeDataResources(resources);
        }
    }
    private static IReadOnlyDictionary<string, decimal?> GetAccountAmounts(IDictionary<string, object> record)
    {
        var accountAmounts = new Dictionary<string, decimal?>();
        foreach (var accountName in new string[]
        {
            "総資産",
            "純資産",
            "総負債",
            "売上高",
            "売上総利益",
            "営業利益",
            "経常利益",
            "親会社帰属利益",
            "営業活動によるキャッシュフロー",
            "配当金"
        })
        {
            var amountStr = record[accountName]?.ToString() ?? throw new Exception($"{accountName}の値が不正です: {record[accountName]}");
            if (amountStr == "NA")
            {
                accountAmounts.Add(accountName, null);
            }
            else
            {
                accountAmounts.Add(accountName, ModifyAmounts(accountName, decimal.Parse(amountStr)));
            }
        }

        // 金額が重複している項目があるとうまく名寄せができないので削除する
        RemoveDuplicateValues(accountAmounts);

        return accountAmounts;
    }
    private static void RemoveDuplicateValues(in Dictionary<string, decimal?> accountAmounts)
    {
        var memo = new HashSet<decimal?>();
        foreach (var value in accountAmounts.Values)
        {
            if (value is not null && memo.Contains(value))
            {
                var keysOfDuplicateValues = accountAmounts
                    .Where(x => x.Value == value)
                    .Select(x => x.Key);
                foreach (var key in keysOfDuplicateValues)
                {
                    accountAmounts.Remove(key);
                }
            }
            memo.Add(value);
        }
    }
    private static decimal ModifyAmounts(string accountName, decimal amounts) => accountName switch
    {
        "売上高" => amounts * 1000000,
        "総資産" => amounts * 1000000,
        "純資産" => amounts * 1000000,
        "経常利益" => amounts * 1000000,
        "営業利益" => amounts * 1000000,
        "親会社帰属利益" => amounts * 1000000,
        "売上総利益" => amounts * 1000000,
        "営業活動によるキャッシュフロー" => amounts * 1000000,
        "総負債" => amounts * 1000000,
        _ => amounts
    };
    private static int ParseInt(string? maybeStr, string columnName)
    {
        if (!int.TryParse(maybeStr, out var num))
        {
            throw new Exception($"{columnName}が不正です: {maybeStr}");
        }
        return num;
    }
    private static void DisposeDataResources(IEnumerable<IDisposable> resources)
    {
        foreach (var resource in resources)
        {
            resource.Dispose();
        }
    }
}
