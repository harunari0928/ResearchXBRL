using System;
using System.IO;
using System.Linq;
using ResearchXBRL.Application.DTO;
using ResearchXBRL.Infrastructure.Services.TaxonomyParsers;
using Xunit;

namespace ResearchXBRL.Tests.Infrastructure.Service
{
    public sealed class TaxonomyParserTests
    {
        public sealed class ParseTests
        {
            [Fact]
            public void タクソノミの勘定項目スキーマXSDファイルとラベルXMLファイルから全ての会計項目を読み取る()
            {
                // arrange
                var (schema, label) = GetStreams();
                var accountElementReader = new TaxonomyParser();

                // act
                var source = new EdinetTaxonomyData
                {
                    LabelDataStream = label,
                    SchemaDataStream = schema,
                    TaxonomyVersion = DateTime.Parse("2011/01/05"),
                    Classification = "jpigp"
                };
                var accountElements = accountElementReader.Parse(source)
                    ?? throw new Exception("XML読み込み失敗");

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
                Assert.Equal(source.TaxonomyVersion, actual.TaxonomyVersion);
                Assert.Equal(source.Classification, actual.Classification);

                schema.Dispose();
                label.Dispose();
            }

            [Fact]
            public void 内閣府令項目を取らない()
            {
                // arrange
                var (schema, label) = GetStreams();
                var accountElementReader = new TaxonomyParser();

                // act
                var source = new EdinetTaxonomyData
                {
                    LabelDataStream = label,
                    SchemaDataStream = schema,
                    TaxonomyVersion = DateTime.Parse("2011/01/05"),
                    Classification = "jpigp"
                };
                var accountElements = accountElementReader.Parse(source)
                    ?? throw new Exception("XML読み込み失敗");

                // assert
                Assert.Empty(accountElements.Where(x => x.Classification == "jpcrp"));

                schema.Dispose();
                label.Dispose();
            }

            private (Stream schema, Stream label) GetStreams()
            {
                return (new StreamReader("jppfs_cor_2019-11-01.xsd").BaseStream,
                    new StreamReader("jppfs_2019-11-01_lab.xml").BaseStream);
            }
        }
    }
}
