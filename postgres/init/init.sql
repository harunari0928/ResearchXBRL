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