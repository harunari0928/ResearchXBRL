set client_encoding = 'UTF8';

create table report_covers (
  id uuid primary key,
  document_title varchar not null,
  company_name varchar not null,
  submission_date date not null
);

create table units (
  id uuid primary key,
  unit_name varchar not null,
  unit_type integer not null,
  measure varchar null,
  unit_numerator varchar null,
  unit_denominator  varchar null
);

create table contexts (
  id uuid primary key,
  context_name varchar not null,
  period_type integer not null,
  period_from date null,
  period_to date null,
  instant_date date null
);

create table report_items (
  id uuid primary key,
  classification varchar not null,
  xbrl_name varchar not null,
  amounts decimal null,
  numerical_accuracy decimal null,
  scale decimal null,
  unit_name varchar not null,
  context_name varchar not null
);
