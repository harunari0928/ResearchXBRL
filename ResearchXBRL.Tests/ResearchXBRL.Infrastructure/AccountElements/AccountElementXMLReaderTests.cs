using ResearchXBRL.Infrastructure.AccountElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace ResearchXBRL.Tests.ResearchXBRL.Infrastructure.AccountElements
{
    public sealed class AccountElementXMLReaderTests
    {
        public sealed class ReadTests : IDisposable
        {
            private readonly MemoryStream schemaStream = new();
            private TextWriter? schemaWriter;
            private readonly MemoryStream labelStream = new();
            private TextWriter? labelWriter;

            [Fact]
            public void タクソノミの勘定項目スキーマXSDファイルとラベルXMLファイルから全ての会計項目を読み取る()
            {
                // arrange
                CreateSchemaDocument();
                CreateLabelDocument();

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
            }

            private void CreateSchemaDocument()
            {
                schemaWriter = new StreamWriter(schemaStream);
                schemaWriter.WriteLine(@"
<?xml version=""1.0"" encoding=""UTF - 8""?>
  <!--(c)Financial Services Agency, The Japanese Government.
      All Rights Reserved.
      For intellectual property policy and authorized uses of EDINET Taxonomy,
      refer to “EDINET Taxonomy Legal Statement” at http://www.fsa.go.jp/search/EDINET_Taxonomy_Legal_Statement.html
-->
<xsd:schema targetNamespace=""http://disclosure.edinet-fsa.go.jp/taxonomy/jppfs/2019-11-01/jppfs_cor"" attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:jppfs_cor=""http://disclosure.edinet-fsa.go.jp/taxonomy/jppfs/2019-11-01/jppfs_cor"" xmlns:iod=""http://disclosure.edinet-fsa.go.jp/taxonomy/common/2013-08-31/iod"" xmlns:nonnum=""http://www.xbrl.org/dtr/type/non-numeric"" xmlns:link=""http://www.xbrl.org/2003/linkbase"" xmlns:xbrldt=""http://xbrl.org/2005/xbrldt"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"">                           
  <xsd:import namespace=""http://www.xbrl.org/2003/instance"" schemaLocation=""http://www.xbrl.org/2003/xbrl-instance-2003-12-31.xsd""/>
  <xsd:import namespace=""http://disclosure.edinet-fsa.go.jp/taxonomy/jppfs/2012/jppfs_pe"" schemaLocation=""jppfs_pe_2012.xsd""/>
  <xsd:import namespace=""http://www.xbrl.org/2006/ref"" schemaLocation=""http://www.xbrl.org/2006/ref-2006-02-27.xsd""/>
  <xsd:import namespace=""http://www.xbrl.org/dtr/type/non-numeric"" schemaLocation=""http://www.xbrl.org/dtr/type/nonNumeric-2009-12-16.xsd""/>
  <xsd:import namespace=""http://disclosure.edinet-fsa.go.jp/taxonomy/common/2013-08-31/iod"" schemaLocation=""../../common/2013-08-31/identificationAndOrdering_2013-08-31.xsd""/>
  <xsd:import namespace=""http://xbrl.org/2005/xbrldt"" schemaLocation=""http://www.xbrl.org/2005/xbrldt-2005.xsd""/>
  <xsd:element name=""AssetsAbstract"" id=""jppfs_cor_AssetsAbstract"" type=""xbrli:stringItemType"" substitutionGroup=""xbrli:item"" abstract=""true"" nillable=""true"" xbrli:periodType=""duration""/>
  <xsd:element name=""CurrentAssetsAbstract"" id=""jppfs_cor_CurrentAssetsAbstract"" type=""xbrli:stringItemType"" substitutionGroup=""xbrli:item"" abstract=""true"" nillable=""true"" xbrli:periodType=""duration""/>
  <xsd:element name=""CashAndDeposits"" id=""jppfs_cor_CashAndDeposits"" type=""xbrli:monetaryItemType"" substitutionGroup=""xbrli:item"" abstract=""false"" nillable=""true"" xbrli:balance=""debit"" xbrli:periodType=""instant""/>
  <xsd:element name=""NotesAndAccountsReceivableTrade"" id=""jppfs_cor_NotesAndAccountsReceivableTrade"" type=""xbrli:monetaryItemType"" substitutionGroup=""xbrli:item"" abstract=""false"" nillable=""true"" xbrli:balance=""debit"" xbrli:periodType=""instant""/>
  <xsd:element name=""AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade"" id=""jppfs_cor_AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade"" type=""xbrli:monetaryItemType"" substitutionGroup=""xbrli:item"" abstract=""false"" nillable=""true"" xbrli:balance=""debit"" xbrli:periodType=""instant""/>
  <xsd:element name=""NotesAndAccountsReceivableTradeNet"" id=""jppfs_cor_NotesAndAccountsReceivableTradeNet"" type=""xbrli:monetaryItemType"" substitutionGroup=""xbrli:item"" abstract=""false"" nillable=""true"" xbrli:balance=""debit"" xbrli:periodType=""instant""/>
</xsd:schema>
".Trim());
                schemaWriter.Flush();
            }

            private void CreateLabelDocument()
            {
                labelWriter = new StreamWriter(labelStream);
                labelWriter.WriteLine(@"
<?xml version=""1.0"" encoding=""UTF-8""?>
<!--  (c) Financial Services Agency, The Japanese Government.
      All Rights Reserved.
      For intellectual property policy and authorized uses of EDINET Taxonomy,
      refer to “EDINET Taxonomy Legal Statement” at http://www.fsa.go.jp/search/EDINET_Taxonomy_Legal_Statement.html
-->
<link:linkbase xmlns:link=""http://www.xbrl.org/2003/linkbase"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"">
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jpcrp/std/alt/role/label"" xlink:type=""simple"" xlink:href=""../../../jpcrp/2019-11-01/jpcrp_rt_2019-11-01.xsd#rol_std_altLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/Consolidated/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/Consolidated/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedNegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/Consolidated/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/Consolidated/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedInterim/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedInterimLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedInterim/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedInterimNegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedInterim/role/periodEndLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedInterimPeriodEndLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedInterim/role/periodStartLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedInterimPeriodStartLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedInterim/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedInterimPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedInterim/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedInterimTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedQuarterly/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedQuarterlyLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedQuarterly/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedQuarterlyNegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedQuarterly/role/periodEndLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedQuarterlyPeriodEndLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedQuarterly/role/periodStartLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedQuarterlyPeriodStartLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedQuarterly/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedQuarterlyPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ConsolidatedQuarterly/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ConsolidatedQuarterlyTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedInterim/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedInterimLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedInterim/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedInterimNegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedInterim/role/periodEndLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedInterimPeriodEndLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedInterim/role/periodStartLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedInterimPeriodStartLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedInterim/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedInterimPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedInterim/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedInterimTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedQuarterly/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedQuarterlyLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedQuarterly/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedQuarterlyNegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedQuarterly/role/periodEndLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedQuarterlyPeriodEndLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedQuarterly/role/periodStartLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedQuarterlyPeriodStartLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedQuarterly/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedQuarterlyPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/NonConsolidatedQuarterly/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_NonConsolidatedQuarterlyTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/bnk/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_BNKLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/bnk/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_BNKNegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/bnk/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_BNKPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/bnk/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_BNKTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/cmd/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_CMDLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/cna/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_CNALabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/cns/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_CNSLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/cte/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_CTELabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/edu/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_EDULabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/edu/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_EDUNegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/edu/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_EDUPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/edu/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_EDUTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/elc/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ELCLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/elc/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ELCTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ele/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ELELabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ele/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ELENegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ele/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ELEPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ele/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_ELETotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/fnd/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_FNDLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/fnd/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_FNDTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/gas/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_GASLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/gas/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_GASTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/hwy/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_HWYLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/hwy/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_HWYTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ins/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_INSLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ins/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_INSTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/inv/NonConsolidatedInterim/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_INVNonConsolidatedInterimLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/inv/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_INVLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ivt/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_IVTLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/ivt/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_IVTTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/lea/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_LEALabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/liq/NonConsolidatedInterim/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_LIQNonConsolidatedInterimLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/liq/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_LIQLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/liq/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_LIQTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/med/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_MEDLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/med/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_MEDTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/rwy/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_RWYLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/rwy/role/negativeLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_RWYNegativeLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/rwy/role/positiveLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_RWYPositiveLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/rwy/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_RWYTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/sec/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_SECLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/sec/role/totalLabel"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_SECTotalLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/spf/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_SPFLabel""/>
  <link:roleRef roleURI=""http://disclosure.edinet-fsa.go.jp/jppfs/wat/role/label"" xlink:type=""simple"" xlink:href=""../jppfs_rt_2019-11-01.xsd#rol_WATLabel""/>
  <link:labelLink xlink:type=""extended"" xlink:role=""http://www.xbrl.org/2003/role/link"">
    <link:loc xlink:type=""locator"" xlink:href=""../jppfs_cor_2019-11-01.xsd#jppfs_cor_AssetsAbstract"" xlink:label=""AssetsAbstract""/>
    <link:label xlink:type=""resource"" xlink:label=""label_AssetsAbstract"" xlink:role=""http://www.xbrl.org/2003/role/label"" xml:lang=""ja"" id=""label_AssetsAbstract"">資産の部</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""AssetsAbstract"" xlink:to=""label_AssetsAbstract""/>
    <link:label xlink:type=""resource"" xlink:label=""label_AssetsAbstract_2"" xlink:role=""http://www.xbrl.org/2003/role/verboseLabel"" xml:lang=""ja"" id=""label_AssetsAbstract_2"">資産の部 [タイトル項目]</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""AssetsAbstract"" xlink:to=""label_AssetsAbstract_2""/>
    <link:loc xlink:type=""locator"" xlink:href=""../jppfs_cor_2019-11-01.xsd#jppfs_cor_CurrentAssetsAbstract"" xlink:label=""CurrentAssetsAbstract""/>
    <link:label xlink:type=""resource"" xlink:label=""label_CurrentAssetsAbstract"" xlink:role=""http://www.xbrl.org/2003/role/label"" xml:lang=""ja"" id=""label_CurrentAssetsAbstract"">流動資産</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""CurrentAssetsAbstract"" xlink:to=""label_CurrentAssetsAbstract""/>
    <link:label xlink:type=""resource"" xlink:label=""label_CurrentAssetsAbstract_2"" xlink:role=""http://www.xbrl.org/2003/role/verboseLabel"" xml:lang=""ja"" id=""label_CurrentAssetsAbstract_2"">流動資産 [タイトル項目]</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""CurrentAssetsAbstract"" xlink:to=""label_CurrentAssetsAbstract_2""/>
    <link:loc xlink:type=""locator"" xlink:href=""../jppfs_cor_2019-11-01.xsd#jppfs_cor_CashAndDeposits"" xlink:label=""CashAndDeposits""/>
    <link:label xlink:type=""resource"" xlink:label=""label_CashAndDeposits"" xlink:role=""http://www.xbrl.org/2003/role/label"" xml:lang=""ja"" id=""label_CashAndDeposits"">現金及び預金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""CashAndDeposits"" xlink:to=""label_CashAndDeposits""/>
    <link:label xlink:type=""resource"" xlink:label=""label_CashAndDeposits_2"" xlink:role=""http://www.xbrl.org/2003/role/verboseLabel"" xml:lang=""ja"" id=""label_CashAndDeposits_2"">現金及び預金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""CashAndDeposits"" xlink:to=""label_CashAndDeposits_2""/>
    <link:label xlink:type=""resource"" xlink:label=""label_CashAndDeposits_3"" xlink:role=""http://disclosure.edinet-fsa.go.jp/jppfs/cns/role/label"" xml:lang=""ja"" id=""label_CashAndDeposits_3"">現金預金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""CashAndDeposits"" xlink:to=""label_CashAndDeposits_3""/>
    <link:label xlink:type=""resource"" xlink:label=""label_CashAndDeposits_4"" xlink:role=""http://disclosure.edinet-fsa.go.jp/jppfs/ivt/role/label"" xml:lang=""ja"" id=""label_CashAndDeposits_4"">現金・預金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""CashAndDeposits"" xlink:to=""label_CashAndDeposits_4""/>
    <link:label xlink:type=""resource"" xlink:label=""label_CashAndDeposits_5"" xlink:role=""http://disclosure.edinet-fsa.go.jp/jppfs/sec/role/label"" xml:lang=""ja"" id=""label_CashAndDeposits_5"">現金・預金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""CashAndDeposits"" xlink:to=""label_CashAndDeposits_5""/>
    <link:loc xlink:type=""locator"" xlink:href=""../jppfs_cor_2019-11-01.xsd#jppfs_cor_NotesAndAccountsReceivableTrade"" xlink:label=""NotesAndAccountsReceivableTrade""/>
    <link:label xlink:type=""resource"" xlink:label=""label_NotesAndAccountsReceivableTrade"" xlink:role=""http://www.xbrl.org/2003/role/label"" xml:lang=""ja"" id=""label_NotesAndAccountsReceivableTrade"">受取手形及び売掛金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""NotesAndAccountsReceivableTrade"" xlink:to=""label_NotesAndAccountsReceivableTrade""/>
    <link:label xlink:type=""resource"" xlink:label=""label_NotesAndAccountsReceivableTrade_2"" xlink:role=""http://www.xbrl.org/2003/role/verboseLabel"" xml:lang=""ja"" id=""label_NotesAndAccountsReceivableTrade_2"">受取手形及び売掛金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""NotesAndAccountsReceivableTrade"" xlink:to=""label_NotesAndAccountsReceivableTrade_2""/>
    <link:loc xlink:type=""locator"" xlink:href=""../jppfs_cor_2019-11-01.xsd#jppfs_cor_AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade"" xlink:label=""AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade""/>
    <link:label xlink:type=""resource"" xlink:label=""label_AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade"" xlink:role=""http://www.xbrl.org/2003/role/label"" xml:lang=""ja"" id=""label_AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade"">貸倒引当金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade"" xlink:to=""label_AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade""/>
    <link:label xlink:type=""resource"" xlink:label=""label_AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade_2"" xlink:role=""http://www.xbrl.org/2003/role/verboseLabel"" xml:lang=""ja"" id=""label_AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade_2"">貸倒引当金、受取手形及び売掛金</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade"" xlink:to=""label_AllowanceForDoubtfulAccountsNotesAndAccountsReceivableTrade_2""/>
    <link:loc xlink:type=""locator"" xlink:href=""../jppfs_cor_2019-11-01.xsd#jppfs_cor_NotesAndAccountsReceivableTradeNet"" xlink:label=""NotesAndAccountsReceivableTradeNet""/>
    <link:label xlink:type=""resource"" xlink:label=""label_NotesAndAccountsReceivableTradeNet"" xlink:role=""http://www.xbrl.org/2003/role/label"" xml:lang=""ja"" id=""label_NotesAndAccountsReceivableTradeNet"">受取手形及び売掛金（純額）</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""NotesAndAccountsReceivableTradeNet"" xlink:to=""label_NotesAndAccountsReceivableTradeNet""/>
    <link:label xlink:type=""resource"" xlink:label=""label_NotesAndAccountsReceivableTradeNet_2"" xlink:role=""http://www.xbrl.org/2003/role/verboseLabel"" xml:lang=""ja"" id=""label_NotesAndAccountsReceivableTradeNet_2"">受取手形及び売掛金（純額）</link:label>
    <link:labelArc xlink:type=""arc"" xlink:arcrole=""http://www.xbrl.org/2003/arcrole/concept-label"" xlink:from=""NotesAndAccountsReceivableTradeNet"" xlink:to=""label_NotesAndAccountsReceivableTradeNet_2""/>
  </link:labelLink>
</link:linkbase>
".Trim());
                labelWriter.Flush();
            }

            private (TextReader schemaReader, TextReader labelReader) GetReaders()
            {
                schemaStream.Position = labelStream.Position = 0;
                return (new StreamReader(schemaStream), new StreamReader(labelStream));
            }

            public void Dispose()
            {
                schemaWriter?.Dispose();
                labelWriter?.Dispose();
            }
        }
    }
}
