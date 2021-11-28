using CsvHelper;
using ResearchXBRL.Domain.AccountElements;
using ResearchXBRL.Infrastructure.AccountElements;
using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.AccountElements
{
    public sealed class AccountElementCSVWriterTests
    {
        public sealed class WriteTests : IDisposable
        {
            private readonly MemoryStream memoryStream;

            public WriteTests()
            {
                lock (this)
                {
                    memoryStream = new();
                }
            }

            [Fact]
            public async Task ヘッダが出力される()
            {
                // arrange
                var elements = new AccountElement[]
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
                        TaxonomyVersion = DateTime.Parse("2021/01/02"),
                        Classification = "jpigp"
                    },
                };
                var (writer, reader) = GetStreamWriterReader();
                using var accountElementWriter = new AccountElementCSVWriter(writer);
                using var csvReader = new CsvReader(reader, CultureInfo.CurrentCulture);

                // act
                await accountElementWriter.Write(elements);
                ReadyForRead();
                csvReader.Read();

                // assert
                Assert.True(csvReader.ReadHeader());
            }

            [Fact]
            public async Task 引数の会計項目全てがcsv出力される()
            {
                // arrange
                var expected = new AccountElement[]
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
                        TaxonomyVersion = DateTime.Parse("2021/01/02"),
                        Classification = "jpffs"
                    },
                    new AccountElement
                    {
                        AccountName = "前受け金",
                        XBRLName = "test2",
                        Type = "test-type2",
                        Balance = "aaa",
                        PeriodType = "aaa",
                        TaxonomyVersion = DateTime.Parse("2019/11/12"),
                        Classification = "jpigp"
                    },
                    new AccountElement
                    {
                        AccountName = "前受け金41",
                    },
                };
                var (writer, reader) = GetStreamWriterReader();
                using var csvReader = new CsvReader(reader, CultureInfo.CurrentCulture);
                using var accountElementWriter = new AccountElementCSVWriter(writer);

                // act
                await accountElementWriter.Write(expected);
                ReadyForRead();
                var actual = csvReader.GetRecords<AccountElement>();

                // assert
                Assert.Equal(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(actual));
            }

            private (StreamWriter writer, StreamReader reader) GetStreamWriterReader()
            {
                return (new StreamWriter(memoryStream)
                {
                    AutoFlush = true
                }, new StreamReader(memoryStream));
            }

            private void ReadyForRead()
            {
                lock (this)
                {
                    memoryStream.Position = 0;
                }
            }

            public void Dispose()
            {
                memoryStream.Dispose();
            }
        }
    }
}
