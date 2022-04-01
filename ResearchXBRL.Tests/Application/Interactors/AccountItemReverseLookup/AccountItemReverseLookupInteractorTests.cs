using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using ResearchXBRL.Application.DTO.AccountItemReverseLookup;
using ResearchXBRL.Application.Interactors.AccountItemReverseLookup;
using ResearchXBRL.Application.QueryServices.AccountItemReverseLookup;
using ResearchXBRL.Domain.AccountItemReverseLookup.AccountItems;
using System.Linq;
using Xunit;

namespace ResearchXBRL.Tests.Application.Interactors.AccountItemReverseLookup;

public sealed class AccountItemReverseLookupInteractorTests
{
    public sealed class HandleTests
    {
        private readonly Mock<IReverseDictionaryQueryService> reverseDictionaryQueryServiceMock = new();
        private readonly Mock<IReverseLookupQueryService> reverseLookupQueryService = new();
        private readonly Mock<IAccountItemRepository> repository = new();

        [Fact(DisplayName = "逆引き辞書の要素数だけ逆引きを行う")]
        public async Task Test1()
        {
            // arrange
            var lookupParameters = new List<ReverseLookupParameters>
            {
                new ReverseLookupParameters
                {
                    SecuritiesCode = 1111,
                    NetSales = 100
                },
                new ReverseLookupParameters
                {
                    SecuritiesCode = 1112,
                    NetSales = 101
                },
            };
            reverseDictionaryQueryServiceMock
                .Setup(x => x.Get())
                .Returns(lookupParameters);
            var reverseLookupResult = CreateReverseLookupQueryServiceMock();
            var interactor = new AccountItemReverseLookupInteractor(
                reverseDictionaryQueryServiceMock.Object,
                reverseLookupQueryService.Object,
                repository.Object
            );

            // act
            await interactor.Handle();

            // assert
            reverseLookupQueryService
                .Verify(x => x.Lookup(It.IsAny<ReverseLookupParameters>()),
                Times.Exactly(lookupParameters.Count), "逆引き辞書の要素数だけ逆引きを行う");
        }

        [Fact(DisplayName = "逆引き辞書をもとに逆引きを行う")]
        public async Task Test2()
        {
            // arrange
            var lookupParameters = new List<ReverseLookupParameters>
            {
                new ReverseLookupParameters
                {
                    SecuritiesCode = 1111,
                    NetSales = 100
                },
                new ReverseLookupParameters
                {
                    SecuritiesCode = 1112,
                    NetSales = 101
                },
            };
            reverseDictionaryQueryServiceMock
                .Setup(x => x.Get())
                .Returns(lookupParameters);
            var reverseLookupResult = CreateReverseLookupQueryServiceMock();
            var interactor = new AccountItemReverseLookupInteractor(
                reverseDictionaryQueryServiceMock.Object,
                reverseLookupQueryService.Object,
                repository.Object
            );

            // act
            await interactor.Handle();

            // assert
            reverseLookupQueryService
                .Verify(x => x.Lookup(It.Is<ReverseLookupParameters>(p => p == lookupParameters[0])),
                Times.Once);
            reverseLookupQueryService
                .Verify(x => x.Lookup(It.Is<ReverseLookupParameters>(p => p == lookupParameters[1])),
                Times.Once);
        }

        [Fact(DisplayName = "逆引き結果をリポジトリへ登録する")]
        public async Task Test3()
        {
            // arrange
            var lookupParameters = new List<ReverseLookupParameters>
            {
                new ReverseLookupParameters
                {
                    SecuritiesCode = 1111,
                    NetSales = 100
                },
            };
            reverseDictionaryQueryServiceMock
                .Setup(x => x.Get())
                .Returns(lookupParameters);
            var reverseLookupResult = CreateReverseLookupQueryServiceMock();
            var interactor = new AccountItemReverseLookupInteractor(
                reverseDictionaryQueryServiceMock.Object,
                reverseLookupQueryService.Object,
                repository.Object
            );

            // act
            await interactor.Handle();

            // assert
            repository
            .Verify(x =>
                x.Add(
                    It.Is<IEnumerable<AccountItem>>(
                        x => x.ElementAt(0).NormalizedName == reverseLookupResult[0].NormalizedName
                         && x.ElementAt(0).OriginalName == reverseLookupResult[0].OriginalName))
                , Times.Once);
            repository
            .Verify(x =>
                x.Add(
                    It.Is<IEnumerable<AccountItem>>(
                        x => x.ElementAt(1).NormalizedName == reverseLookupResult[1].NormalizedName
                         && x.ElementAt(1).OriginalName == reverseLookupResult[1].OriginalName))
                , Times.Once);
        }

        private IReadOnlyList<ReverseLookupResult> CreateReverseLookupQueryServiceMock()
        {
            var reverseLookupResult = new List<ReverseLookupResult>
            {
                new ReverseLookupResult
                {
                    NormalizedName = "NetSales",
                    OriginalName = "hoge"
                },
                new ReverseLookupResult
                {
                    NormalizedName = "ROE",
                    OriginalName = "fuga"
                }
            };
            reverseLookupQueryService
                .Setup(x => x.Lookup(It.IsAny<ReverseLookupParameters>()))
                .ReturnsAsync(reverseLookupResult);
            return reverseLookupResult;
        }
    }
}
