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

    private static async IAsyncEnumerable<FinancialReport> ReadFinancialReports(CsvReader reader, IEnumerable<IDisposable> dataResources)
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
        DisposeDataResources(dataResources);
    }
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
