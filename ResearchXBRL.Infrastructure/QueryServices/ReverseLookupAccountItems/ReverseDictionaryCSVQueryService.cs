using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using ResearchXBRL.Application.DTO.ReverseLookupAccountItems;
using ResearchXBRL.Application.DTO.Results;
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
        return new Success<IAsyncEnumerable<FinancialReport>>(ReadFinancialReports(reader,
            new IDisposable[] { reader, streamReader, fileStream }));
    }

    private static async IAsyncEnumerable<FinancialReport> ReadFinancialReports(CsvReader reader, IEnumerable<IDisposable> resources)
    {
        try
        {
            await foreach (IDictionary<string, object> record in reader.GetRecordsAsync<dynamic>())
            {
                var accountAmounts = GetAccountAmounts(record);

                var accountingStandard = (AccountingStandards)int.Parse(record["会計基準"]?.ToString()
                    ?? throw new Exception($"会計基準が不正です: {record["会計基準"]}"));

                if (record["証券コード"]?.ToString() == "NA")
                {
                    continue; // 未上場企業は無視
                }

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
    private static IReadOnlyDictionary<string, decimal> GetAccountAmounts(IDictionary<string, object> record)
    {
        var accountAmounts = new Dictionary<string, decimal>();
        foreach (var (accountName, accountNameInCsv) in new (string, string)[] {
                ("NetSales", "売上高"),
                ("TotalAssets", "総資産"),
                ("NetAssets", "純資産"),
                ("OrdinaryIncome", "経常利益"),
                ("OperatingIncome", "営業利益"),
                ("ProfitLossAttributableToOwnersOfParent", "親会社帰属利益"),
                ("GrossProfit", "売上総利益"),
                ("NetCashProvidedByUsedInOperatingActivities", "営業活動によるキャッシュフロー"),
                ("Liabilities", "総負債"),
                ("DividendPaidPerShareSummaryOfBusinessResults", "配当金")
            })
        {
            var amountStr = record[accountNameInCsv]?.ToString() ?? throw new Exception($"{accountNameInCsv}が不正です: {record[accountNameInCsv]}");
            if (amountStr != "NA")
            {
                accountAmounts.Add(accountName, ModifyAmounts(accountNameInCsv, decimal.Parse(amountStr)));
            }
        }

        return accountAmounts;
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
