#!/usr/bin/env bash

DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-postgres}"
DB_USER="${DB_USER:-postgres}"
DB_PASS="${DB_PASS:-postgres}"
CONN_STRING="Server=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASS};"

if [ -z "$ConnectionStrings__Index" ];
then
    echo "Setting ConnectionStrings__Index to $CONN_STRING"
    ConnectionStrings__Index=$CONN_STRING
fi

export ConnectionStrings__Index

python3 set_cors_hosts.py appsettings.json

dotnet Anna.Api.dll