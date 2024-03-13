#!/bin/bash

/init-cql.sh &
(exec /docker-entrypoint.py "$@")