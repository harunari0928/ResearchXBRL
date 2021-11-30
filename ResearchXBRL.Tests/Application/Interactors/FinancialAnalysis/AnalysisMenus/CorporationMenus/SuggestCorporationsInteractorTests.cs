using System;
using Moq;
using Xunit;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ResearchXBRL.Tests.Application.Interactors.FinancialAnalysis.AnalysisMenus.CorporationMenus
{
    public sealed class SuggestCorporationsInteractorTests
    {
        private readonly Mock<ICorporationMenuRepository> repository;

        public SuggestCorporationsInteractorTests()
        {
            this.repository = new();
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
                    EdinetId = "test"
                },
                new Corporation
                {
                    Name = "企業B",
                    EdinetId = "test2"
                },
                new Corporation
                {
                    Name = "企業C",
                    EdinetId = "test3"
                }
            };
            var keyword = "キーワード";
            repository
                .Setup(x => x.GetProposals(keyword))
                .ReturnsAsync(new CorporatonMenu
                {
                    Corporations = expected
                });
            var interactor = new SuggestCorporationsInteractor(repository.Object);

            // act
            var actual = await interactor.Handle(keyword);

            // assert
            foreach (var (e, a) in expected.Zip(actual))
            {
                Assert.Equal(e.Name, a.Name);
                Assert.StrictEqual(e.EdinetId, a.EdinetId);
            }
        }

        [Fact]
        public async Task 渡されたキーワードが空文字のときサジェストしない()
        {
            // arrange
            var keyword = ""; // キーワードが空文字
            repository
                .Setup(x => x.GetProposals(keyword))
                .ReturnsAsync(new CorporatonMenu
                {
                    Corporations = new List<Corporation>
                    {
                        new Corporation
                        {
                            Name = "企業A",
                            EdinetId = "test"
                        },
                        new Corporation
                        {
                            Name = "企業B",
                            EdinetId = "test2"
                        },
                        new Corporation
                        {
                            Name = "企業C",
                            EdinetId = "test3"
                        }
                    }
                });
            var interactor = new SuggestCorporationsInteractor(repository.Object);

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
                .ReturnsAsync(new CorporatonMenu
                {
                    Corporations = new List<Corporation>
                    {
                        new Corporation
                        {
                            Name = "企業A",
                            EdinetId = "test"
                        },
                        new Corporation
                        {
                            Name = "企業B",
                            EdinetId = "test2"
                        },
                        new Corporation
                        {
                            Name = "企業C",
                            EdinetId = "test3"
                        }
                    }
                });
            var interactor = new SuggestCorporationsInteractor(repository.Object);

            // act
            var actual = await interactor.Handle(keyword);

            // assert
            Assert.Empty(actual);
        }
    }
}
