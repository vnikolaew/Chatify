#!/bin/bash

exec /docker-entrypoint.py "$@"

CQL_INIT="/init.cql"
echo "Executing: $CQL_INIT"

until cqlsh -f "$CQL_INIT"; do
    echo "Unavailable: sleeping"
    sleep 10
done &
