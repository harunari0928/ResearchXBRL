#!/bin/bash
set -e

echo "rdb_primary:5432:replication:replication_user:replicationpassword" > /.pgpass
chmod 600 /.pgpass

if [ ! -s "$PGDATA/PG_VERSION" ]; then
    until ping -c 1 -W 1 rdb_primary
    do
        echo "Waiting for primary to ping..."
        sleep 1s
    done

    until pg_basebackup -h rdb_primary -D ${PGDATA} -U replication_user -vP -W --progress -R
    do
        echo "Waiting for primary to connect..."
        sleep 1s
    done

    sed -i 's/wal_level = hot_standby/wal_level = replica/g' ${PGDATA}/postgresql.conf

    cat >> ${PGDATA}/postgresql.conf <<EOF
primary_conninfo = 'host=rdb_primary port=5432 user=replication_user application_name=rdb_readonly'
primary_slot_name = 'node_a_slot'
EOF

    chown postgres:postgres ${PGDATA} -R
    chmod 700 ${PGDATA} -R
fi
