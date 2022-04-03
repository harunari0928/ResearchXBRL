using System;
using System.IO;
using Xunit;
using ResearchXBRL.Infrastructure.Services.FileStorages;
using ResearchXBRL.Infrastructure.QueryServices.AccountItemReverseLookup;
using System.Linq;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;
using System.Threading.Tasks;

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
        var actual = await service.Get().ElementAtAsync(3);

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
        var actual = await service.Get().ElementAtAsync(2);

        // assert
        Assert.Null(actual.NetSales);
    }
}
