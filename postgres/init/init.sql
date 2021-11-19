set client_encoding = 'UTF8';

create table report_covers (
  id uuid primary key,
  document_title varchar not null,
  company_name varchar not null,
  submission_date date not null
);

create table units (
  report_id uuid primary key,
  unit_name varchar primary key,
  unit_type integer not null,
  measure varchar null,
  unit_numerator varchar null,
  unit_denominator  varchar null
);

create table contexts (
  report_id uuid primary key,
  context_name varchar primary key,
  period_type integer not null,
  period_from date null,
  period_to date null,
  instant_date date null
);

create table report_items (
  report_id uuid primary key,
  classification varchar primary key,
  xbrl_name varchar primary key,
  amounts decimal null,
  numerical_accuracy decimal null,
  scale decimal null,
  unit_name varchar not null,
  context_name varchar not null
);
