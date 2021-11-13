using ResearchXBRL.Infrastructure.AccountElements;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.AccountElements
{
    public sealed class AccountElementXMLReaderTests
    {
        public sealed class ReadTests
        {
            
            [Fact]
            public void タクソノミの勘定項目スキーマXSDファイルとラベルXMLファイルから全ての会計項目を読み取る()
            {
                // arrange
                var (schemaReader, lableReader) = GetReaders();
                using var accountElementReader = new AccountElementXMLReader(schemaReader, lableReader);

                // act
                var accountElements = accountElementReader.Read() ?? throw new Exception("XML読み込み失敗");

                // assert
                var actual = accountElements.ElementAt(3);
                Assert.Equal("NotesAndAccountsReceivableTrade", actual.XBRLName);
                Assert.Equal("受取手形及び売掛金", actual.AccountName);
                Assert.Equal("debit", actual.Balance);
                Assert.False(actual.Abstract);
                Assert.True(actual.Nillable);
                Assert.Equal("instant", actual.PeriodType);
                Assert.Equal("xbrli:item", actual.SubstitutionGroup);
                Assert.Equal("xbrli:monetaryItemType", actual.Type);
                Assert.False(actual.Abstract);
                Assert.Equal(DateTime.Parse("2019-11-01"), actual.TaxonomyVersion);

                schemaReader.Dispose();
                lableReader.Dispose();
            }

          

            private (TextReader schemaReader, TextReader labelReader) GetReaders()
            {
                return (new StreamReader("jppfs_cor_2019-11-01.xsd"),
                    new StreamReader("jppfs_2019-11-01_lab.xml"));
            }
        }
    }
}
