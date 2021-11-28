using Moq;
using ResearchXBRL.Domain.AccountElements;
using ResearchXBRL.Application.AccountElements;
using System.Threading.Tasks;
using Xunit;
using ResearchXBRL.Application.Usecase.AccountElements.Transfer;
using System.Text.Json;
using System.Collections.Generic;
using System;
using System.IO;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;

namespace ResearchXBRL.Tests.Application.AccountElements
{
    public sealed class TransferAccountElementsInteractorTests
    {
        public sealed class HandleTests
        {
            private readonly Mock<ITaxonomyParser> accountElementReader;
            private readonly Mock<IAccountElementWriter> accountElementWriter;
            private readonly Mock<ITransferAccountElementsPresenter> presenter;

            public HandleTests()
            {
                accountElementReader = new();
                accountElementWriter = new();
                presenter = new();
            }

            [Fact]
            public async Task 読み込んだ会計項目全てを書き込む()
            {
                // arrange
                var expectedAccountElements = new AccountElement[]
                {
                    new AccountElement
                    {
                        AccountName = "売掛金",
                        XBRLName = "test",
                        Type = "test-type",
                        SubstitutionGroup = "group-test",
                        Abstract = true,
                        Nillable = true,
                        Balance = "aaa",
                        PeriodType = "",
                        TaxonomyVersion = DateTime.Parse("2021/01/02")
                    },
                    new AccountElement
                    {
                        AccountName = "前受け金",
                        XBRLName = "test2",
                        Type = "test-type2",
                        Balance = "aaa",
                        PeriodType = "aaa",
                        TaxonomyVersion = DateTime.Parse("2019/11/12")
                    },
                    new AccountElement
                    {
                        AccountName = "前受け金41",
                    },
                };
                accountElementReader
                    .Setup(x => x.Parse(It.IsAny<EdinetTaxonomyData>()))
                    .Returns(expectedAccountElements);
                var interactor = CreateInteractor();

                // act
                await interactor.Hundle(new MemoryStream(0), new MemoryStream(0));

                // assert                
                accountElementWriter
                    .Verify(x => x.Write(It.Is<IEnumerable<AccountElement>>(x =>
                    JsonSerializer.Serialize(expectedAccountElements, default)
                    == JsonSerializer.Serialize(x, default)))
                    , Times.Once);
            }

            [Fact]
            public async Task 書き込み完了後に完了処理を行う()
            {
                // arrange
                var isWritingDone = false;
                accountElementWriter
                    .Setup(x => x.Write(It.IsAny<IEnumerable<AccountElement>>()))
                    .Callback(() => isWritingDone = true);
                var interactor = CreateInteractor();

                // act
                await interactor.Hundle(new MemoryStream(0), new MemoryStream(0));

                // assert                
                presenter
                    .Setup(x => x.Complete())
                    .Callback(() => Assert.True(isWritingDone));
            }

            private TransferAccountElementsInteractor CreateInteractor()
            {
                return new TransferAccountElementsInteractor(
                    accountElementReader.Object,
                    accountElementWriter.Object,
                    presenter.Object);
            }
        }
    }
}
