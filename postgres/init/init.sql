set client_encoding = 'UTF8';

create table report_covers (
  id uuid primary key,
  document_title varchar not null,
  company_name varchar not null,
  submission_date date not null
);

create table units (
  report_id uuid,
  unit_name varchar,
  unit_type integer not null,
  measure varchar null,
  unit_numerator varchar null,
  unit_denominator  varchar null,
  PRIMARY KEY (report_id, unit_name)
);

create table contexts (
  report_id uuid,
  context_name varchar,
  period_type integer not null,
  period_from date null,
  period_to date null,
  instant_date date null,
  PRIMARY KEY (report_id, context_name)
);

create table report_items (
  report_id uuid,
  classification varchar,
  xbrl_name varchar,
  amounts decimal null,
  numerical_accuracy decimal null,
  scale decimal null,
  unit_name varchar not null,
  context_name varchar not null,
  PRIMARY KEY (report_id, classification, xbrl_name)
);
