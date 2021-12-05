#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
CREATE ROLE replication_user LOGIN REPLICATION PASSWORD 'replicationpassword';
EOSQL

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
SELECT * FROM pg_create_physical_replication_slot('node_a_slot');
EOSQL

mkdir $PGDATA/archive

cat >> "$PGDATA/postgresql.conf" <<EOF
wal_level = hot_standby
max_wal_senders = 10
max_replication_slots = 10
synchronous_commit = off
EOF

echo "host replication replication_user 0.0.0.0/0 trust" >> "$PGDATA/pg_hba.conf"
