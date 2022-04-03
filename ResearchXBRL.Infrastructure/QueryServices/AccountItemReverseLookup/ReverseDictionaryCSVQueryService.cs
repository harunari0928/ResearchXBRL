using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;
using ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;
using ResearchXBRL.Infrastructure.Services;

namespace ResearchXBRL.Infrastructure.QueryServices.AccountItemReverseLookup;

public sealed class ReverseDictionaryCSVQueryService : IReverseDictionaryQueryService
{
    private readonly IFileStorage fileStorage;
    private readonly string filePath;

    public ReverseDictionaryCSVQueryService(IFileStorage fileStorage, string filePath)
    {
        this.fileStorage = fileStorage;
        this.filePath = filePath;
    }

    public async IAsyncEnumerable<FinancialReport> Get()
    {
        using var fileStream = fileStorage.Get(filePath);
        using var streamReader = new StreamReader(fileStream);
        using var reader = new CsvReader(streamReader, CultureInfo.CurrentCulture, true);
        // return ReadFinancialReports(reader);としてもビルドは通るが、
        // 遅延実行中にreaderのDisposeが走ってしまい実行時エラーになる
        await foreach (var report in ReadFinancialReports(reader))
        {
            yield return report;
        }
    }

    private static async IAsyncEnumerable<FinancialReport> ReadFinancialReports(CsvReader reader)
    {
        await foreach (IDictionary<string, object> record in reader.GetRecordsAsync<dynamic>())
        {
            var netSalesStr = record["売上高"]?.ToString() ?? throw new Exception($"売上高が不正です: {record["売上高"]}");
            decimal? maybeNetsales = netSalesStr == "NA" ? null : decimal.Parse(netSalesStr);

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
                NetSales = maybeNetsales
            };
        }
    }

    private static int ParseInt(string? maybeStr, string columnName)
    {
        if (!int.TryParse(maybeStr, out var num))
        {
            throw new Exception($"{columnName}が不正です: {maybeStr}");
        }
        return num;
    }
}
