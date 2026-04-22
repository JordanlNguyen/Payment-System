using System.Collections.Generic;
using Npgsql;
using Banks;

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
    }
}