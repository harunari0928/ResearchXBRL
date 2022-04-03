using System;
using Xunit;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using ResearchXBRL.Infrastructure.QueryServices.AccountItemReverseLookup;
using System.Linq;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.Results;
using System.Collections.Generic;

namespace ResearchXBRL.Tests.Infrastructure.QueryService;

public sealed class ReverseDictionaryCSVQueryServiceTests
{
    private readonly LocalFileStorage fileStorage = new(".");

    [Fact(DisplayName = "csvのデータを取得できる")]
    public async Task Test2()
    {
        // arrange
        var service = new ReverseDictionaryCSVQueryService(fileStorage, "ReverseLookupDictionary.csv");

        // act
        if (service.Get() is not Success<IAsyncEnumerable<FinancialReport>> success)
        {
            throw new Exception("Test Failed.");
        }

        var actual = await success.Value.ElementAtAsync(3);

        // assert
        Assert.Equal(1301, actual.SecuritiesCode);
        Assert.Equal(AccountingStandards.Japanese, actual.AccountingStandard);
        Assert.Equal(new DateOnly(2020, 3, 31), actual.FiscalYear);
        Assert.Equal(262519, actual.NetSales);
    }

    [Fact(DisplayName = "NAのデータはnullとして取得する")]
    public async Task Test3()
    {
        // arrange
        var service = new ReverseDictionaryCSVQueryService(fileStorage, "ReverseLookupDictionary.csv");

        // act
        if (service.Get() is not Success<IAsyncEnumerable<FinancialReport>> success)
        {
            throw new Exception("Test Failed.");
        }
        var actual = await success.Value.ElementAtAsync(2);

        // assert
        Assert.Null(actual.NetSales);
    }
}
