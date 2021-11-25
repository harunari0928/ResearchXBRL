set client_encoding = 'UTF8';

create table report_covers (
  id varchar primary key,
  company_id varchar not null,
  document_type varchar not null,
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
