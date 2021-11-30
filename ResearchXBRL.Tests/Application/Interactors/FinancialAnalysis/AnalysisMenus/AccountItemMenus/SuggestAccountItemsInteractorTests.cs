using Moq;
using System.Threading.Tasks;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using Xunit;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using System.Collections.Generic;
using System.Linq;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.AnalysisMenus.AccountItemMenus
{
    public sealed class SuggestAccountItemsInteractorTests
    {
        private readonly Mock<IAccountItemMenuRepository> repository;

        public SuggestAccountItemsInteractorTests()
        {
            repository = new();
        }

        [Fact]
        public async Task 渡されたキーワードをもとにサジェスト対象会計項目を返す()
        {
            // arrange
            var expected = new List<AccountItem>
            {
                new AccountItem
                {
                    Name = "売掛金",
                    XBRLNames = new string[] { "URIKAKE1", "URIKAKE2" }
                },
                new AccountItem
                {
                    Name = "買掛金",
                    XBRLNames = new string[] { "KAIKAKE1", "KAIKAKE2", "KAIKAKE3" }
                },
                new AccountItem
                {
                    Name = "雑費",
                    XBRLNames = new string[] { "ZAPPI1", "ZAPPI2", "ZAPPI3" }
                }
            };
            var keyword = "キーワード";
            repository
                .Setup(x => x.GetProposals(keyword))
                .ReturnsAsync(new AccountItemMenu
                {
                    AccountItems = expected
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
                .ReturnsAsync(new AccountItemMenu
                {
                    AccountItems = new List<AccountItem>
                    {
                        new AccountItem
                        {
                            Name = "売掛金",
                            XBRLNames = new string[] { "URIKAKE1", "URIKAKE2" }
                        },
                        new AccountItem
                        {
                            Name = "買掛金",
                            XBRLNames = new string[] { "KAIKAKE1", "KAIKAKE2", "KAIKAKE3" }
                        },
                        new AccountItem
                        {
                            Name = "雑費",
                            XBRLNames = new string[] { "ZAPPI1", "ZAPPI2", "ZAPPI3" }
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
                .ReturnsAsync(new AccountItemMenu
                {
                    AccountItems = new List<AccountItem>
                    {
                        new AccountItem
                        {
                            Name = "売掛金",
                            XBRLNames = new string[] { "URIKAKE1", "URIKAKE2" }
                        },
                        new AccountItem
                        {
                            Name = "買掛金",
                            XBRLNames = new string[] { "KAIKAKE1", "KAIKAKE2", "KAIKAKE3" }
                        },
                        new AccountItem
                        {
                            Name = "雑費",
                            XBRLNames = new string[] { "ZAPPI1", "ZAPPI2", "ZAPPI3" }
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
