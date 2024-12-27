#!/usr/bin/env bash

set -e

cat migrations.sql | sqlite3 $ANNA_INDEX_DB_PATH

dotnet Anna.Api.dll
