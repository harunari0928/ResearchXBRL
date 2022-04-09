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
                    AccountAmounts = GetAccountAmounts(record)
                };
            }
        }
        finally
        {
            DisposeDataResources(resources);
        }
    }
    private static IReadOnlyDictionary<string, (decimal? amounts, int priority)> GetAccountAmounts(IDictionary<string, object> record)
    {
        var accountAmounts = new Dictionary<string, (decimal? amounts, int priority)>();
        foreach (var (accountName, accountNameInCsv, priority) in new (string, string, int)[]
        {
            // 金額が大きくなりがちなものから順に優先度を昇順に並べること
            // 逆引き結果重複を防ぐため
            ("TotalAssets", "総資産", 1),
            ("NetAssets", "純資産", 2),
            ("Liabilities", "総負債", 3),
            ("NetSales", "売上高", 4),
            ("GrossProfit", "売上総利益", 5),
            ("OperatingIncome", "営業利益", 6),
            ("OrdinaryIncome", "経常利益", 7),
            ("ProfitLossAttributableToOwnersOfParent", "親会社帰属利益", 8),
            ("NetCashProvidedByUsedInOperatingActivities", "営業活動によるキャッシュフロー", 9),
            ("DividendPaidPerShareSummaryOfBusinessResults", "配当金", 10)
        })
        {
            var amountStr = record[accountNameInCsv]?.ToString() ?? throw new Exception($"{accountNameInCsv}が不正です: {record[accountNameInCsv]}");
            if (amountStr == "NA")
            {
                accountAmounts.Add(accountName, (null, priority));
            }
            else
            {
                accountAmounts.Add(accountName, (ModifyAmounts(accountNameInCsv, decimal.Parse(amountStr)), priority));
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
