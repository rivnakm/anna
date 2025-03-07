#!/usr/bin/env bash

DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-postgres}"
DB_USER="${DB_USER:-postgres}"
DB_PASS="${DB_PASS:-postgres}"
ConnectionStrings__Index="${ConnectionStrings__Index:-Server=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASS};}"

dotnet Anna.Api.dll