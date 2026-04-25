using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using Banks;
using Bank1Backend.Models;

namespace Bank1Backend.Repositories
{
    public class TransactionRepository
    {
        private readonly Bank _bank;
        public TransactionRepository(Bank bank)
        {
            _bank = bank;
        }

        // function takes input of accountNumber, uses registered singleton, and thensearch database for rows having that accountNumber
        public List<Dictionary<string, object>> FindRowsByAccountNumber(string accountNumber)
        {
            var results = new List<Dictionary<string, object>>();
            using var conn = new NpgsqlConnection(_bank.ConnectionString);

            conn.Open();
            string query = "SELECT * FROM user_accounts WHERE account_number = @acc";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@acc", accountNumber);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(row);
            }
            conn.Close();
            return results;
        }

        public decimal GetAccountBalance(string accountNumber)
        {
            decimal balance = 0;
            using var conn = new NpgsqlConnection(_bank.ConnectionString);
            conn.Open();
            string query = "SELECT balance FROM user_accounts WHERE account_number = @acc";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@acc", accountNumber);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                balance = reader.GetDecimal(0);
            }
            conn.Close();
            return balance;
        }

        public void UpdateAccountBalance(string accountNumber, decimal newBalance)
        {
            using var conn = new NpgsqlConnection(_bank.ConnectionString);
            conn.Open();
            string query = "UPDATE user_accounts SET balance = @balance WHERE account_number = @acc";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@balance", newBalance);
            cmd.Parameters.AddWithValue("@acc", accountNumber);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void PostTransactionInHistory(Guid transactionId, Dictionary<string, object> customerAccountInfo, decimal amount, TransactionRequestModel request, DateTime transactionDate)
        {
            // insert into transactionhistory
            using var conn = new NpgsqlConnection(_bank.ConnectionString);
            var customerId = Guid.Empty;
            if (customerAccountInfo.TryGetValue("customer_user_id", out object? customerUserIdObj))
            {
                if (customerUserIdObj is Guid id)
                {
                    customerId = id;
                }
                else if (customerUserIdObj is string idText && Guid.TryParse(idText, out var parsedId))
                {
                    customerId = parsedId;
                }
            }

            var accountNumber = customerAccountInfo.TryGetValue("account_number", out object? accNumObj)
                ? accNumObj?.ToString() ?? string.Empty
                : string.Empty;

            var merchantName = request.MerchantName;
            Guid? merchantId = Guid.TryParse(request.MerchantId, out var parsedMerchantId) ? parsedMerchantId : null;
            const string status = "posted";

            if (customerId == Guid.Empty)
            {
                throw new InvalidOperationException("customer_user_id is missing or invalid in customerAccountInfo.");
            }

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                throw new InvalidOperationException("account_number is missing in customerAccountInfo.");
            }
            
            conn.Open();
            string query = "INSERT INTO transactionhistory (id, customer_user_id, account_number, merchant_name, merchant_id, amount, transaction_date, status) VALUES (@transactionId, @customerUserId, @accountNumber, @merchantName, @merchantId, @amount, @transactionDate, @status)";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@transactionId", transactionId);
            cmd.Parameters.AddWithValue("@customerUserId", customerId);
            cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
            cmd.Parameters.AddWithValue("@merchantName", merchantName);
            if (merchantId.HasValue)
            {
                cmd.Parameters.AddWithValue("@merchantId", merchantId.Value);
            }
            else
            {
                cmd.Parameters.Add("@merchantId", NpgsqlDbType.Uuid).Value = DBNull.Value;
            }
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@transactionDate", transactionDate);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // updates transfer fund pool
        public bool UpdateTransferPool(decimal amount)
        {
            /*
            1. create connection to database table of poolAmount
            2. if withdrawing, ensure that there is enough money in the pool to be transferred out
            3. update the pool table
            NOTE: amount is positive if adding and negative if withdrawing
            */
            // create connection to database
            using var conn = new NpgsqlConnection(_bank.ConnectionString);
            conn.Open();
            string querySelect = "SELECT amount FROM poolamount WHERE id = 1";
            using var cmdSelect = new NpgsqlCommand(querySelect, conn);
            decimal currentPoolAmount = 0;
            using var reader = cmdSelect.ExecuteReader();
            if (reader.Read())
            {
                currentPoolAmount = reader.GetDecimal(0);
            }
            reader.Close();

            if (currentPoolAmount < amount)
            {
                // not enough money in the pool to transfer out
                return false;
            }

            decimal newPoolAmount = currentPoolAmount + amount;
            string queryUpdate = "UPDATE poolamount SET amount = @newAmount WHERE id = 1";
            using var cmdUpdate = new NpgsqlCommand(queryUpdate, conn);
            cmdUpdate.Parameters.AddWithValue("@newAmount", newPoolAmount);
            cmdUpdate.ExecuteNonQuery();
            conn.Close();
            return true;
        }

        // adds info (amount, destination account, merchant name, merchant id, and status of transfer (pending or completed) to transfer pool table so that at the end of the day, the bank can process all transfer requests in the pool and update the status of each transfer request accordingly)
        public void AddTransferPool(TransactionRequestModel request, Guid transactionId, DateTime transactionDate)
        {
            using var conn = new NpgsqlConnection(_bank.ConnectionString);
            conn.Open();
            string query = "INSERT INTO transfer_requests (transactionid, amount, transaction_date, source_account, destination_account, destination_routing_number, merchant_name, merchant_id) VALUES (@transactionid, @amount, @transactionDate, @sourceAccount, @destinationAccount, @destinationRoutingNumber, @merchantName, @merchantId)";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@transactionid", transactionId);
            cmd.Parameters.AddWithValue("@sourceAccount", request.SourceAccount);
            cmd.Parameters.AddWithValue("@destinationAccount", request.DestinationAccount);
            cmd.Parameters.AddWithValue("@amount", request.Amount);
            cmd.Parameters.AddWithValue("@destinationRoutingNumber", request.DestinationRoutingNumber);
            cmd.Parameters.AddWithValue("@transactionDate", transactionDate);
            cmd.Parameters.AddWithValue("@merchantName", request.MerchantName);
            cmd.Parameters.AddWithValue("@merchantId", request.MerchantId);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public List<Dictionary<string, object>> GetPendingTransferPoolRows()
        {
            var results = new List<Dictionary<string, object>>();
            using var conn = new NpgsqlConnection(_bank.ConnectionString);
            conn.Open();

            const string query = "SELECT * FROM transfer_pool WHERE status = @status";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@status", "pending");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }

                results.Add(row);
            }

            return results;
        }
    }
}