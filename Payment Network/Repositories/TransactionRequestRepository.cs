using Npgsql;
using NpgsqlTypes;

namespace PaymentNetwork.Repositories
{
    public class TransactionRequestRepository
    {
        private readonly string _connectionString;

        public TransactionRequestRepository()
        {
            var envVars = new Dictionary<string, string>();
            var lines = File.ReadAllLines("EnvironmentVariables.txt");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    envVars[parts[0].Trim()] = parts[1].Trim();
                }
            }

            string dbHost = envVars.ContainsKey("DB_HOST") ? envVars["DB_HOST"] : "";
            string dbPort = envVars.ContainsKey("DB_PORT") ? envVars["DB_PORT"] : "";
            string dbUser = envVars.ContainsKey("DB_USERNAME") ? envVars["DB_USERNAME"] : "";
            string dbPass = envVars.ContainsKey("DB_PASSWORD") ? envVars["DB_PASSWORD"] : "";
            string dbName = envVars.ContainsKey("DB_NAME") ? envVars["DB_NAME"] : "";

            _connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPass};Database={dbName}";
        }

        public bool AreBothBanksInNetwork(string sourceRoutingNumber, string destinationRoutingNumber)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = @"SELECT COUNT(DISTINCT routing_number)
                                   FROM banks_in_network
                                   WHERE routing_number = @sourceRoutingNumber
                                      OR routing_number = @destinationRoutingNumber";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@sourceRoutingNumber", sourceRoutingNumber);
            cmd.Parameters.AddWithValue("@destinationRoutingNumber", destinationRoutingNumber);

            int matchCount = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();

            return matchCount == 2;
        }

        public string SearchForUrl(string routingNumber)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            const string query = @"SELECT url_endpoint_for_receiving_transaction
                                   FROM banks_in_network
                                   WHERE routing_number = @routingNumber";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@routingNumber", routingNumber);

            var url = cmd.ExecuteScalar();
            conn.Close();

            return url?.ToString() ?? string.Empty;
        }
    }
}