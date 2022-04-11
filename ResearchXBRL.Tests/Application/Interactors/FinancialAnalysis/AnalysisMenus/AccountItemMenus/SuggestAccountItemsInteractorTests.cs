using Moq;
using System.Threading.Tasks;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using Xunit;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItems;
using System.Collections.Generic;
using System.Linq;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public sealed class SuggestAccountItemsInteractorTests
    {
        private readonly Mock<IAccountItemsMenuRepository> repository;

        public SuggestAccountItemsInteractorTests()
        {
            repository = new();
        }

        [Fact]
        public async Task 渡されたキーワードをもとにサジェスト対象勘定項目を返す()
        {
            // arrange
            var expected = new List<AccountItem>
            {
                new AccountItem
                {
                    Name = "売掛金",
                },
                new AccountItem
                {
                    Name = "買掛金",
                },
                new AccountItem
                {
                    Name = "雑費",
                }
            };
            var keyword = "売掛金";
            repository
                .Setup(x => x.GetProposals(keyword))
                .ReturnsAsync(new AccountItemsMenu
                {
                    SearchedAccountItem = expected.First(),
                    SuggestedAccountItems = expected.Skip(1).ToArray()
                });
            var interactor = new SuggestAccountItemsInteractor(repository.Object);

            // act
            var actual = await interactor.Handle(keyword);

            // assert
            foreach (var (e, a) in expected.Zip(actual))
            {
                Assert.Equal(e.Name, a.Name);
            }
        }

        [Fact]
        public async Task 渡されたキーワードと同じ名前の勘定項目がサジェストの先頭にくる()
        {
            // arrange
            var expected = new List<AccountItem>
            {
                new AccountItem
                {
                    Name = "売掛金",
                },
                new AccountItem
                {
                    Name = "買掛金",
                },
                new AccountItem
                {
                    Name = "雑費",
                }
            };
            var keyword = "売掛金";
            repository
                .Setup(x => x.GetProposals(keyword))
                .ReturnsAsync(new AccountItemsMenu
                {
                    SearchedAccountItem = expected.First(),
                    SuggestedAccountItems = expected.Skip(1).ToArray()
                });
            var interactor = new SuggestAccountItemsInteractor(repository.Object);

            // act
            var actual = await interactor.Handle(keyword);

            // assert
            Assert.Equal(keyword, actual.First().Name);
        }

        [Fact]
        public async Task 渡されたキーワードと同じ勘定項目が存在しない場合でもサジェスト対象会計項目を返す()
        {
            // arrange
            var expected = new List<AccountItem>
            {
                new AccountItem
                {
                    Name = "売掛金",
                },
                new AccountItem
                {
                    Name = "買掛金",
                },
                new AccountItem
                {
                    Name = "雑費",
                }
            };
            var keyword = "売掛";
            repository
                .Setup(x => x.GetProposals(keyword))
                .ReturnsAsync(new AccountItemsMenu
                {
                    SearchedAccountItem = null,
                    SuggestedAccountItems = expected.ToArray()
                });
            var interactor = new SuggestAccountItemsInteractor(repository.Object);

            // act
            var actual = await interactor.Handle(keyword);

            // assert
            foreach (var (e, a) in expected.Zip(actual))
            {
                Assert.Equal(e.Name, a.Name);
            }
        }

        [Fact]
        public async Task 渡されたキーワードが空文字のときサジェストしない()
        {
            // arrange
            var keyword = ""; // キーワードが空文字
            repository
                .Setup(x => x.GetProposals(keyword))
                .ReturnsAsync(new AccountItemsMenu
                {
                    SuggestedAccountItems = new List<AccountItem>
                    {
                        new AccountItem
                        {
                            Name = "売掛金",
                        },
                        new AccountItem
                        {
                            Name = "買掛金",
                        },
                        new AccountItem
                        {
                            Name = "雑費",
                        }
                    }
                });
            var interactor = new SuggestAccountItemsInteractor(repository.Object);

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
            repository
                .Setup(x => x.GetProposals(keyword))
                .ReturnsAsync(new AccountItemsMenu
                {
                    SuggestedAccountItems = new List<AccountItem>
                    {
                        new AccountItem
                        {
                            Name = "売掛金",
                        },
                        new AccountItem
                        {
                            Name = "買掛金",
                        },
                        new AccountItem
                        {
                            Name = "雑費",
                        }
                    }
                });
            var interactor = new SuggestAccountItemsInteractor(repository.Object);

            // act
            var actual = await interactor.Handle(keyword);

            // assert
            Assert.Empty(actual);
        }
    }
}
