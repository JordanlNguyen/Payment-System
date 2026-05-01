\set ON_ERROR_STOP on

SELECT 'CREATE DATABASE "Bank1DB"'
WHERE NOT EXISTS (
    SELECT 1
    FROM pg_database
    WHERE datname = 'Bank1DB'
)\gexec

SELECT 'CREATE DATABASE paymentsystem'
WHERE NOT EXISTS (
    SELECT 1
    FROM pg_database
    WHERE datname = 'paymentsystem'
)\gexec

\connect "Bank1DB"

CREATE TABLE IF NOT EXISTS user_accounts (
    customer_user_id uuid PRIMARY KEY,
    account_number varchar(32) NOT NULL UNIQUE,
    routing_number varchar(32) NOT NULL,
    account_holder_name varchar(120) NOT NULL,
    balance numeric(18, 2) NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS transactionhistory (
    id uuid PRIMARY KEY,
    customer_user_id uuid NOT NULL,
    account_number varchar(32) NOT NULL,
    merchant_name varchar(120) NOT NULL,
    merchant_id uuid NULL,
    amount numeric(18, 2) NOT NULL,
    transaction_date timestamp with time zone NOT NULL,
    status varchar(32) NOT NULL,
    CONSTRAINT fk_transactionhistory_user_accounts
        FOREIGN KEY (customer_user_id) REFERENCES user_accounts (customer_user_id)
);

CREATE TABLE IF NOT EXISTS poolamount (
    id integer PRIMARY KEY,
    amount numeric(18, 2) NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS transfer_requests (
    transactionid uuid PRIMARY KEY,
    amount numeric(18, 2) NOT NULL,
    transaction_date timestamp with time zone NOT NULL,
    source_account varchar(32) NOT NULL,
    destination_account varchar(32) NOT NULL,
    destination_routing_number varchar(32) NOT NULL,
    merchant_name varchar(120) NOT NULL,
    merchant_id varchar(64) NOT NULL
);

CREATE TABLE IF NOT EXISTS transfer_pool (
    transactionid uuid PRIMARY KEY,
    amount numeric(18, 2) NOT NULL,
    transaction_date timestamp with time zone NOT NULL,
    source_account varchar(32) NOT NULL,
    destination_account varchar(32) NOT NULL,
    destination_routing_number varchar(32) NOT NULL,
    merchant_name varchar(120) NOT NULL,
    merchant_id varchar(64) NOT NULL,
    status varchar(32) NOT NULL DEFAULT 'pending'
);

INSERT INTO poolamount (id, amount)
VALUES (1, 100000.00)
ON CONFLICT (id) DO UPDATE
SET amount = EXCLUDED.amount;

INSERT INTO user_accounts (
    customer_user_id,
    account_number,
    routing_number,
    account_holder_name,
    balance
)
VALUES
    (
        '11111111-1111-1111-1111-111111111111',
        '123456789',
        '0917381183',
        'Sample Customer',
        2500.00
    ),
    (
        '22222222-2222-2222-2222-222222222222',
        '987654321',
        '0917381183',
        'Backup Customer',
        900.00
    )
ON CONFLICT (account_number) DO UPDATE
SET routing_number = EXCLUDED.routing_number,
    account_holder_name = EXCLUDED.account_holder_name,
    balance = EXCLUDED.balance;

\connect paymentsystem

CREATE TABLE IF NOT EXISTS banks_in_network (
    routing_number varchar(32) PRIMARY KEY,
    bank_name varchar(120) NOT NULL,
    url_endpoint_for_receiving_transaction text NOT NULL
);

INSERT INTO banks_in_network (
    routing_number,
    bank_name,
    url_endpoint_for_receiving_transaction
)
VALUES
    (
        '0917381183',
        'Chase',
        'http://localhost:5058/chaseApi/TransactionRequest/validateTransaction'
    ),
    (
        '222000111',
        'MerchantBank',
        'http://localhost:5058/chaseApi/TransactionRequest/validateTransaction'
    )
ON CONFLICT (routing_number) DO UPDATE
SET bank_name = EXCLUDED.bank_name,
    url_endpoint_for_receiving_transaction = EXCLUDED.url_endpoint_for_receiving_transaction;