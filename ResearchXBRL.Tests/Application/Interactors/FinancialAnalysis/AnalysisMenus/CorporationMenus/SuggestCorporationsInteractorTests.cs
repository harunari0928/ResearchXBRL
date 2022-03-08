using Moq;
using Xunit;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.Corporations;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.AnalysisMenus.CorporationMenus;

public sealed class SuggestCorporationsInteractorTests
{
    private readonly Mock<ICorporationsMenuRepository> repositoryMock;

    public SuggestCorporationsInteractorTests()
    {
        this.repositoryMock = new();
    }

    [Fact]
    public async Task 渡されたキーワードをもとにサジェスト対象企業を返す()
    {
        // arrange
        var expected = new List<Corporation>
            {
                new Corporation
                {
                    Name = "企業A",
                    CorporationId = "test"
                },
                new Corporation
                {
                    Name = "企業B",
                    CorporationId = "test2"
                },
                new Corporation
                {
                    Name = "企業C",
                    CorporationId = "test3"
                }
            };
        var keyword = "キーワード";
        repositoryMock
            .Setup(x => x.GetProposals(keyword))
            .ReturnsAsync(new CorporatonsMenu
            {
                Corporations = expected
            });
        var interactor = new SuggestCorporationsInteractor(repositoryMock.Object);

        // act
        var actual = await interactor.Handle(keyword);

        // assert
        foreach (var (e, a) in expected.Zip(actual))
        {
            Assert.Equal(e.Name, a.Name);
            Assert.Equal(e.CorporationId, a.CorporationId);
        }
    }

    [Fact]
    public async Task 渡されたキーワードが空文字のときサジェストしない()
    {
        // arrange
        var keyword = ""; // キーワードが空文字
        repositoryMock
            .Setup(x => x.GetProposals(keyword))
            .ReturnsAsync(new CorporatonsMenu
            {
                Corporations = new List<Corporation>
                {
                        new Corporation
                        {
                            Name = "企業A",
                            CorporationId = "test"
                        },
                        new Corporation
                        {
                            Name = "企業B",
                            CorporationId = "test2"
                        },
                        new Corporation
                        {
                            Name = "企業C",
                            CorporationId = "test3"
                        }
                }
            });
        var interactor = new SuggestCorporationsInteractor(repositoryMock.Object);

        // act
        var actual = await interactor.Handle(keyword);

        // assert
        Assert.Empty(actual);
    }

    [Fact]
    public async Task 渡されたキーワードが空白のときサジェストしない()
    {
        // arrange
        var keyword = " 　"; // キーワードが空白
        repositoryMock
            .Setup(x => x.GetProposals(keyword))
            .ReturnsAsync(new CorporatonsMenu
            {
                Corporations = new List<Corporation>
                {
                        new Corporation
                        {
                            Name = "企業A",
                            CorporationId = "test"
                        },
                        new Corporation
                        {
                            Name = "企業B",
                            CorporationId = "test2"
                        },
                        new Corporation
                        {
                            Name = "企業C",
                            CorporationId = "test3"
                        }
                }
            });
        var interactor = new SuggestCorporationsInteractor(repositoryMock.Object);

        // act
        var actual = await interactor.Handle(keyword);

        // assert
        Assert.Empty(actual);
    }

    [Fact]
    public async Task 株式会社というワードではサジェストしない()
    {
        // arrange
        var keyword = "株式会社"; // キーワードが空白
        repositoryMock
            .Setup(x => x.GetProposals(keyword))
            .ReturnsAsync(new CorporatonsMenu
            {
                Corporations = new List<Corporation>
                {
                    new Corporation
                    {
                        Name = "企業A",
                        CorporationId = "test"
                    },
                    new Corporation
                    {
                        Name = "企業B",
                        CorporationId = "test2"
                    },
                    new Corporation
                    {
                        Name = "企業C",
                        CorporationId = "test3"
                    }
                }
            });
        var interactor = new SuggestCorporationsInteractor(repositoryMock.Object);

        // act
        var actual = await interactor.Handle(keyword);

        // assert
        Assert.Empty(actual);
    }

    [Fact(DisplayName = "4文字の数字が渡されたとき、それを証券コードとみなし検索し、検索ヒットした1件の企業のみ返す")]
    public async Task Test5()
    {
        // arrange
        var keyword = "1376"; // キーワードが証券コード
        var expected = new Corporation
        {
            CorporationId = "E00004",
            Name = "カネコ種苗株式会社"
        };
        repositoryMock
            .Setup(x => x.FindBySecuritiesCode(keyword))
            .ReturnsAsync(expected);
        repositoryMock
            .Setup(x => x.GetProposals(keyword))
            .ReturnsAsync(new CorporatonsMenu
            {
                Corporations = new List<Corporation>
                {
                    new Corporation
                    {
                        Name = "企業A",
                        CorporationId = "test"
                    },
                    new Corporation
                    {
                        Name = "企業B",
                        CorporationId = "test2"
                    }
                }
            });
        var interactor = new SuggestCorporationsInteractor(repositoryMock.Object);

        // act
        var actual = await interactor.Handle(keyword);

        // assert
        Assert.Single(actual);
        Assert.Equal(expected.CorporationId, actual[0].CorporationId);
        Assert.Equal(expected.Name, actual[0].Name);
    }

    [Fact(DisplayName = "4文字の数字が渡されたとき、それを証券コードとみなし検索する。検索ヒットしなかった場合は空のリストを返す")]
    public async Task Test6()
    {
        // arrange
        var keyword = "1376"; // キーワードが証券コード
        repositoryMock
            .Setup(x => x.FindBySecuritiesCode(keyword))
            .ReturnsAsync(null as Corporation);
        repositoryMock
            .Setup(x => x.GetProposals(keyword))
            .ReturnsAsync(new CorporatonsMenu
            {
                Corporations = new List<Corporation>
                {
                    new Corporation
                    {
                        Name = "企業A",
                        CorporationId = "test"
                    },
                    new Corporation
                    {
                        Name = "企業B",
                        CorporationId = "test2"
                    }
                }
            });
        var interactor = new SuggestCorporationsInteractor(repositoryMock.Object);

        // act
        var actual = await interactor.Handle(keyword);

        // assert
        Assert.Empty(actual);
    }

    [Fact(DisplayName = "3文字の数字が渡されたとき、それを証券コードとみなさない")]
    public async Task Test7()
    {
        // arrange
        var keyword = "136"; // キーワードが証券コードでない
        repositoryMock
            .Setup(x => x.FindBySecuritiesCode(keyword))
            .ReturnsAsync(null as Corporation);
        repositoryMock
            .Setup(x => x.GetProposals(keyword))
            .ReturnsAsync(new CorporatonsMenu
            {
                Corporations = new List<Corporation>()
            });
        var interactor = new SuggestCorporationsInteractor(repositoryMock.Object);

        // act
        var actual = await interactor.Handle(keyword);

        // assert
        repositoryMock.Verify(x => x.FindBySecuritiesCode(keyword), Times.Never);
        repositoryMock.Verify(x => x.GetProposals(keyword), Times.Once);
    }

    [Fact(DisplayName = "5文字の数字が渡されたとき、それを証券コードとみなさない")]
    public async Task Test8()
    {
        // arrange
        var keyword = "13641"; // キーワードが証券コードでない
        repositoryMock
            .Setup(x => x.FindBySecuritiesCode(keyword))
            .ReturnsAsync(null as Corporation);
        repositoryMock
            .Setup(x => x.GetProposals(keyword))
            .ReturnsAsync(new CorporatonsMenu
            {
                Corporations = new List<Corporation>()
            });
        var interactor = new SuggestCorporationsInteractor(repositoryMock.Object);

        // act
        var actual = await interactor.Handle(keyword);

        // assert
        repositoryMock.Verify(x => x.FindBySecuritiesCode(keyword), Times.Never);
        repositoryMock.Verify(x => x.GetProposals(keyword), Times.Once);
    }
}
