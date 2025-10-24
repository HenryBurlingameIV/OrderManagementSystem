#!/bin/bash

set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    SELECT 'CREATE DATABASE catalogdb' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'catalogdb')\gexec
    SELECT 'CREATE DATABASE orderdb' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'orderdb')\gexec
    SELECT 'CREATE DATABASE opsdb' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'opsdb')\gexec
    SELECT 'CREATE DATABASE onsdb' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'onsdb')\gexec
    
    GRANT ALL PRIVILEGES ON DATABASE catalogdb TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE orderdb TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE opsdb TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE onsdb TO "$POSTGRES_USER";
EOSQL