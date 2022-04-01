using Moq;
using ResearchXBRL.Domain.ImportAccountItems.AccountItems;
using System.Threading.Tasks;
using Xunit;
using ResearchXBRL.Application.Usecase.ImportAccountItems.Transfer;
using ResearchXBRL.Application.Interactors.ImportAccountItems.Transfer;
using System.Text.Json;
using System.Collections.Generic;
using System;
using System.IO;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Application.Services;

namespace ResearchXBRL.Tests.Application.Interactors.ImportAccountItems.Transfer
{
    public sealed class TransferAccountItemsInteractorTests
    {
        public sealed class HandleTests
        {
            private readonly Mock<ITaxonomyParser> accountElementReader;
            private readonly Mock<IAccountItemsWriter> accountElementWriter;
            private readonly Mock<ITransferAccountItemsPresenter> presenter;

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
                var expectedAccountElements = new AccountItem[]
                {
                    new AccountItem
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
                    new AccountItem
                    {
                        AccountName = "前受け金",
                        XBRLName = "test2",
                        Type = "test-type2",
                        Balance = "aaa",
                        PeriodType = "aaa",
                        TaxonomyVersion = DateTime.Parse("2019/11/12")
                    },
                    new AccountItem
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
                    .Verify(x => x.Write(It.Is<IEnumerable<AccountItem>>(x =>
                    JsonSerializer.Serialize(expectedAccountElements, new JsonSerializerOptions())
                    == JsonSerializer.Serialize(x, new JsonSerializerOptions())))
                    , Times.Once);
            }

            [Fact]
            public async Task 書き込み完了後に完了処理を行う()
            {
                // arrange
                var isWritingDone = false;
                accountElementWriter
                    .Setup(x => x.Write(It.IsAny<IEnumerable<AccountItem>>()))
                    .Callback(() => isWritingDone = true);
                var interactor = CreateInteractor();

                // act
                await interactor.Hundle(new MemoryStream(0), new MemoryStream(0));

                // assert                
                presenter
                    .Setup(x => x.Complete())
                    .Callback(() => Assert.True(isWritingDone));
            }

            private TransferAccountItemsInteractor CreateInteractor()
            {
                return new TransferAccountItemsInteractor(
                    accountElementReader.Object,
                    accountElementWriter.Object,
                    presenter.Object);
            }
        }
    }
}
