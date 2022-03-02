set client_encoding = 'UTF8';

create table report_covers (
  id varchar primary key,
  company_id varchar not null,
  document_type varchar not null,
  accounting_standards varchar not null,
  submission_date date not null
);

create table units (
  report_id varchar,
  unit_name varchar,
  unit_type integer not null,
  measure varchar null,
  unit_numerator varchar null,
  unit_denominator  varchar null,
  PRIMARY KEY (report_id, unit_name)
);

create table contexts (
  report_id varchar,
  context_name varchar,
  period_type integer not null,
  period_from date null,
  period_to date null,
  instant_date date null,
  PRIMARY KEY (report_id, context_name)
);

create table report_items (
  id uuid primary key,
  report_id varchar,
  classification varchar,
  xbrl_name varchar,
  amounts decimal null,
  numerical_accuracy decimal null,
  scale decimal null,
  unit_name varchar not null,
  context_name varchar not null
);
CREATE INDEX ON report_items (report_id);
CREATE INDEX ON report_items (classification);

create table document_type_master (
  code varchar primary key,
  type_name varchar not null
);

INSERT INTO document_type_master
  (code, type_name)
VALUES
  ('010', '有価証券通知書'),
  ('020', '変更通知書（有価証券通知書）'),
  ('030', '有価証券届出書'),
  ('040', '訂正有価証券届出書'),
  ('050', '届出の取下げ願い'),
  ('060', '発行登録通知書'),
  ('070', '変更通知書（発行登録通知書）'),
  ('080', '発行登録書'),
  ('090', '訂正発行登録書'),
  ('100', '発行登録追補書類'),
  ('110', '発行登録取下届出書'),
  ('120', '有価証券報告書'),
  ('130', '訂正有価証券報告書'),
  ('135', '確認書'),
  ('136', '訂正確認書'),
  ('140', '四半期報告書'),
  ('150', '訂正四半期報告書'),
  ('160', '半期報告書'),
  ('170', '訂正半期報告書'),
  ('180', '臨時報告書'),
  ('190', '訂正臨時報告書'),
  ('200', '親会社等状況報告書'),
  ('210', '訂正親会社等状況報告書'),
  ('220', '自己株券買付状況報告書'),
  ('230', '訂正自己株券買付状況報告書'),
  ('235', '内部統制報告書'),
  ('236', '訂正内部統制報告書'),
  ('240', '公開買付届出書'),
  ('250', '訂正公開買付届出書'),
  ('260', '公開買付撤回届出書'),
  ('270', '公開買付報告書'),
  ('280', '訂正公開買付報告書'),
  ('290', '意見表明報告書'),
  ('300', '訂正意見表明報告書'),
  ('310', '対質問回答報告書'),
  ('320', '訂正対質問回答報告書'),
  ('330', '別途買付け禁止の特例を受けるための申出書'),
  ('340', '訂正別途買付け禁止の特例を受けるための申出書'),
  ('350', '大量保有報告書'),
  ('360', '訂正大量保有報告書'),
  ('370', '基準日の届出書'),
  ('380', '変更の届出書');

create table company_master (
  code varchar primary key,
  submission_type varchar not null,
  is_listed boolean not null,
  is_linking boolean not null,
  capital_amount decimal null,
  settlement_date varchar not null,
  submitter_name varchar not null,
  submitter_name_english varchar not null,
  submitter_name_yomigana varchar not null,
  company_location varchar not null,
  type_of_industry varchar not null,
  securities_code varchar not null,
  corporate_number varchar not null
);

CREATE TABLE account_elements (
  xbrl_name VARCHAR NOT NULL,
  taxonomy_version DATE NOT NULL,
  classification VARCHAR NOT NULL,
  account_name VARCHAR NOT NULL,
  PRIMARY KEY (xbrl_name, taxonomy_version, classification)
);

CREATE TABLE aggregation_of_names_list (
  aggregate_target VARCHAR PRIMARY KEY,
  aggregate_result VARCHAR NOT NULL,
  priority_of_use INT NOT NULL -- 名寄せ元が複数存在したときの優先順位 数字が若いほど優先される
);

INSERT INTO aggregation_of_names_list
  (aggregate_target, aggregate_result, priority_of_use)
VALUES
  -- 売上高
  ('NetSalesSummaryOfBusinessResults', 'NetSales', 1),
  ('OperatingRevenue1', 'NetSales', 2),
  ('ShippingBusinessRevenueWAT', 'NetSales', 3),
  ('ShippingBusinessRevenueAndOtherOperatingRevenueWAT', 'NetSales', 4),
  ('ShippingBusinessRevenueAndOtherServiceRevenueWAT', 'NetSales', 5),
  ('OperatingRevenueELE', 'NetSales', 6),
  ('OperatingRevenueRWY', 'NetSales', 7),
  ('OperatingRevenueSEC', 'NetSales', 8),
  ('ContractsCompletedRevOA', 'NetSales', 9),
  ('NetSalesNS', 'NetSales', 10),
  ('PLJHFDAKJHGF', 'NetSales', 11),
  ('SalesAllSegments', 'NetSales', 12),
  ('SalesDetails', 'NetSales', 13),
  ('TotalSales', 'NetSales', 14),
  ('SalesAndOtherOperatingRevenueSummaryOfBusinessResults', 'NetSales', 15),
  ('NetSalesAndOtherOperatingRevenueSummaryOfBusinessResults', 'NetSales', 16),
  ('NetSalesAndServiceRevenueSummaryOfBusinessResults', 'NetSales', 17),
  ('NetSalesAndOperatingRevenueSummaryOfBusinessResults', 'NetSales', 18),
  ('NetSalesAndOperatingRevenue2SummaryOfBusinessResults', 'NetSales', 19),
  ('NetSalesAndOperatingRevenue', 'NetSales', 20),
  ('NetSalesOfFinishedGoodsRevOA', 'NetSales', 21),
  ('NetSalesOfMerchandiseAndFinishedGoodsRevOA', 'NetSales', 22),
  ('RevenuesUSGAAPSummaryOfBusinessResults', 'NetSales', 23),
  ('NetSalesIFRSSummaryOfBusinessResults', 'NetSales', 24),
  ('TotalTradingTransactionIFRSSummaryOfBusinessResults', 'NetSales', 25),
  ('TotalTradingTransactionsIFRSSummaryOfBusinessResults', 'NetSales', 26),
  ('Revenue', 'NetSales', 27),
  ('RevenueIFRSSummaryOfBusinessResults', 'NetSales', 28),
  ('RevenueSummaryOfBusinessResults', 'NetSales', 29),
  ('OperatingRevenue2', 'NetSales', 30),
  ('OperatingRevenue2SummaryOfBusinessResults', 'NetSales', 31),
  ('GrossOperatingRevenue', 'NetSales', 32),
  ('GrossOperatingRevenueSummaryOfBusinessResults', 'NetSales', 33),
  ('NetSalesRevOA', 'NetSales', 34),
  ('InsurancePremiumsAndOtherOIINS', 'NetSales', 34),
  ('PremiumAndOtherIncomeSummaryOfBusinessResults', 'NetSales', 35),
  ('InsurancePremiumsAndOtherIncomeSummaryOfBusinessResults', 'NetSales', 36),
  ('InsurancePremiumsAndOthersSummaryOfBusinessResults', 'NetSales', 37),
  ('InsurancePremiumsAndOtherSummaryOfBusinessResults', 'NetSales', 38),
  ('WholeChainStoreSalesSummaryOfBusinessResults', 'NetSales', 39),
  ('NetSalesOfCompletedConstructionContractsCNS', 'NetSales', 40),
  ('NetSalesOfCompletedConstructionContractsSummaryOfBusinessResults', 'NetSales', 41),
  ('ContractsCompletedSummaryOfBusinessResults', 'NetSales', 42),
  ('RentIncomeOfRealEstateRevOA', 'NetSales', 43),
  ('OperatingRevenueSPF', 'NetSales', 44),
  ('OperatingRevenueIVT', 'NetSales', 45),
  ('OperatingRevenueCMD', 'NetSales', 46),
  ('OperatingRevenueOILTelecommunications', 'NetSales', 47),
  ('OperatingRevenue1SummaryOfBusinessResults', 'NetSales', 48),
  ('OrdinaryIncomeBNK', 'NetSales', 49),
  ('OperatingIncomeINS', 'NetSales', 50),
  ('OrdinaryIncomeSummaryOfBusinessResults', 'NetSales', 51),
  ('BusinessRevenues', 'NetSales', 52),
  ('BusinessRevenue', 'NetSales', 53),
  ('BusinessRevenueRevOA', 'NetSales', 54),
  ('OperatingRevenue', 'NetSales', 55),
  ('SummaryOfSalesBusinessResults', 'NetSales', 56),
  ('OperatingRevenuesSummaryOfBusinessResults', 'NetSales', 57),
  ('BusinessRevenueSummaryOfBusinessResults', 'NetSales', 58),
  ('OperatingRevenueSummaryOfBusinessResults', 'NetSales', 59),
  -- 営業利益
  ('ProfitLossFromOperatingActivities', 'OperatingIncome', 1),
  ('OperatingIncomeLoss', 'OperatingIncome', 2),
  ('OperatingIncomeLossUSGAAPSummaryOfBusinessResults', 'OperatingIncome', 3),
  ('OperatingProfitLossIFRSSummaryOfBusinessResults', 'OperatingIncome', 4),
  ('OperatingProfitIFRSSummaryOfBusinessResults', 'OperatingIncome', 5),
  ('OperatingIncomeLossIFRSSummaryOfBusinessResults', 'OperatingIncome', 6),
  ('OperatingIncomeIFRSSummaryOfBusinessResults', 'OperatingIncome', 7),
  -- 経常利益
  ('OrdinaryIncomeLossSummaryOfBusinessResults', 'OrdinaryIncome', 1),
  -- 親会社の所有者に帰属する利益
  ('ProfitLossAttributableToOwnersOfParentSummaryOfBusinessResults', 'ProfitLossAttributableToOwnersOfParent', 1),
  ('ProfitLossAndAttributableToOwnersOfParent', 'ProfitLossAttributableToOwnersOfParent', 2),
  -- ROE
  ('RateOfReturnOnEquityUSGAAPSummaryOfBusinessResults', 'RateOfReturnOnEquitySummaryOfBusinessResults', 1),
  ('NetIncomeToSalesBelongingToShareholdersSummaryOfBusinessResults', 'RateOfReturnOnEquitySummaryOfBusinessResults', 2),
  ('RateOfReturnOnEquityIFRSSummaryOfBusinessResults', 'RateOfReturnOnEquitySummaryOfBusinessResults', 3),
  -- EPS
  ('BasicEarningsLossPerShareUSGAAPSummaryOfBusinessResults', 'BasicEarningsLossPerShareSummaryOfBusinessResults', 1),
  ('BasicEarningsLossPerShareIFRSSummaryOfBusinessResults', 'BasicEarningsLossPerShareSummaryOfBusinessResults', 2),
  -- 潜在株式調整後EPS
  ('DilutedEarningsLossPerShareUSGAAPSummaryOfBusinessResults', 'DilutedEarningsPerShareSummaryOfBusinessResults', 1),
  ('DilutedEarningsLossPerShareIFRSSummaryOfBusinessResults', 'DilutedEarningsPerShareSummaryOfBusinessResults', 2),
  -- 1株当たり配当
  ('DividendPaidPerShareFirstSeriesModelAAClassSharesSummaryOfBusinessResults', 'DividendPaidPerShareSummaryOfBusinessResults', 1),
  ('InterimDividendPaidPerShareSummaryOfBusinessResults', 'DividendPaidPerShareSummaryOfBusinessResults', 2),
  ('InterimDividendPaidPerShareFirstSeriesModelAAClassSharesSummaryOfBusinessResults', 'DividendPaidPerShareSummaryOfBusinessResults', 3);
