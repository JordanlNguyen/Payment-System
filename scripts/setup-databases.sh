#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SQL_FILE="$SCRIPT_DIR/setup-databases.sql"

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_USERNAME="${DB_USERNAME:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-}"
DB_ADMIN_DATABASE="${DB_ADMIN_DATABASE:-postgres}"

if ! command -v psql >/dev/null 2>&1; then
    echo "psql was not found. Install PostgreSQL and ensure psql is on PATH before running this script." >&2
    exit 1
fi

if [[ -z "$DB_PASSWORD" ]]; then
    read -r -s -p "PostgreSQL password for $DB_USERNAME: " DB_PASSWORD
    echo
fi

PGPASSWORD="$DB_PASSWORD" psql \
    --host "$DB_HOST" \
    --port "$DB_PORT" \
    --username "$DB_USERNAME" \
    --dbname "$DB_ADMIN_DATABASE" \
    --file "$SQL_FILE"

echo "Databases are ready."