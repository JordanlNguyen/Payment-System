internal static class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("Bank1 Admin CLI — type 'help' for commands, 'exit' to quit.");
        Console.WriteLine();

        //create connection to database, reading the EnvironmentVariables file
        var envVars = new Dictionary<string, string>();
        foreach (var line in File.ReadAllLines("EnvironmentVariables.txt"))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
                envVars[parts[0].Trim()] = parts[1].Trim();
        }
        string dbHost = envVars["DB_HOST"];
        string dbPort = envVars["DB_PORT"];
        string dbUser = envVars["DB_USERNAME"];
        string dbPass = envVars["DB_PASSWORD"];
        string dbName = envVars["DB_NAME"];
        string connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPass};Database={dbName}";
        using var conn = new NpgsqlConnection(connectionString);

        while (true)
        {
            // display admin login
            string adminUsername, adminPassword;
            Console.Write("Admin Username: ");
            adminUsername = Console.ReadLine();
            Console.Write("Admin Password: ");
            adminPassword = Console.ReadLine();

            // authenticate admin
            conn.Open();
            string query = "SELECT password FROM adminusers WHERE username = @username";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", adminUsername);
            var result = cmd.ExecuteReader();
            if (!result.Read() || result.GetString(0) != adminPassword)
            {
                Console.WriteLine("Invalid username or password.");
                continue;
            }
            else if (result.GetString(0) == adminPassword)
            {
                Console.WriteLine("Login successful.");
                DisplayMenuSelection(conn);
            }
        }
            

        return 0;
    }

    private static void DisplayMenuSelection(NpgsqlConnection conn)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Select an option:");
            Console.WriteLine("--- User Management ---");
            Console.WriteLine("  1. Add User");
            Console.WriteLine("  2. Remove User");
            Console.WriteLine("  3. Update User Info");
            Console.WriteLine("  4. View User Info");
            Console.WriteLine("--- Account Management ---");
            Console.WriteLine("  5. Add User Account");
            Console.WriteLine("  6. Remove User Account");
            Console.WriteLine("  7. Update Account Balance");
            Console.WriteLine("  8. Freeze / Unfreeze Account");
            Console.WriteLine("--- Transactions ---");
            Console.WriteLine("  9. View Transaction History");
            Console.WriteLine("  10. View Pending Transfers");
            Console.WriteLine("  11. Cancel Pending Transfer");
            Console.WriteLine("--- Admin ---");
            Console.WriteLine("  12. Add Admin User");
            Console.WriteLine("  13. Remove Admin User");
            Console.WriteLine("  14. Change Admin Password");
            Console.WriteLine("  0. Logout");
            Console.WriteLine();
            Console.Write("> ");
            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    UserFunctions.AddUser(conn);
                    break;
                case "2":
                    UserFunctions.RemoveUser(conn);
                    break;
                case "3":
                    UserFunctions.UpdateUserInfo(conn);
                    break;
                case "4":
                    UserFunctions.ViewUserInfo(conn);
                    break;
                case "5":
                    UserFunctions.AddUserAccount(conn);
                    break;
                case "6":
                    UserFunctions.RemoveUserAccount(conn);
                    break;
                case "7":
                    UserFunctions.UpdateAccountBalance(conn);
                    break;
                case "8":
                    UserFunctions.FreezeUnfreezeAccount(conn);
                    break;
                case "9":
                    UserFunctions.ViewTransactionHistory(conn);
                    break;
                case "10":
                    UserFunctions.ViewPendingTransfers(conn);
                    break;
                case "11":
                    UserFunctions.CancelPendingTransfer(conn);
                    break;
                case "12":
                    AdminFunctions.AddAdminUser(conn);
                    break;
                case "13":
                    AdminFunctions.RemoveAdminUser(conn);
                    break;
                case "14":
                    AdminFunctions.ChangeAdminPassword(conn);
                    break;
                case "0":
                    Console.WriteLine("Logged out.");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please enter 0–14.");
                    break;
            }
        }
    }
}

