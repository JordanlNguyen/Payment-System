namespace Bank1Admin.UserFunctions
{
    public class UserFunctions
    {
        public void AddUser(NpgsqlConnection conn)
        {
            Console.WriteLine("====== ADD USER ======");

            // 1. Collect user information
            Console.Write("First Name: ");
            string? firstName = Console.ReadLine()?.Trim();

            Console.Write("Last Name: ");
            string? lastName = Console.ReadLine()?.Trim();

            Console.Write("Mother's Maiden Name: ");
            string? mothersMaidenName = Console.ReadLine()?.Trim();

            Console.Write("Address: ");
            string? address = Console.ReadLine()?.Trim();

            Console.Write("Phone Number (digits only): ");
            string? phoneNumber = Console.ReadLine()?.Trim();

            Console.Write("Email: ");
            string? email = Console.ReadLine()?.Trim();

            // 2. Validate inputs
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(mothersMaidenName) || string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("All fields are required. User was not added.");
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\d{7,15}$"))
            {
                Console.WriteLine("Invalid phone number. Must be 7–15 digits. User was not added.");
                return;
            }

            if (!email.Contains('@') || !email.Contains('.'))
            {
                Console.WriteLine("Invalid email format. User was not added.");
                return;
            }

            // 3. Check email and phone are not already in the database
            const string checkQuery = @"SELECT COUNT(*) FROM customer_users
                                        WHERE email = @email OR phone_number = @phone";
            using var checkCmd = new NpgsqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@email", email);
            checkCmd.Parameters.AddWithValue("@phone", phoneNumber);
            long existing = (long)(checkCmd.ExecuteScalar() ?? 0L);

            if (existing > 0)
            {
                Console.WriteLine("A user with that email or phone number already exists. User was not added.");
                return;
            }

            //4. display infromation about to be entered and confirm that the admin has the correct info, if not, they can indicate no and they can edit the specified value
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Please review the information below:");
                Console.WriteLine($"  1. First Name:           {firstName}");
                Console.WriteLine($"  2. Last Name:            {lastName}");
                Console.WriteLine($"  3. Mother's Maiden Name: {mothersMaidenName}");
                Console.WriteLine($"  4. Address:              {address}");
                Console.WriteLine($"  5. Phone Number:         {phoneNumber}");
                Console.WriteLine($"  6. Email:                {email}");
                Console.WriteLine();
                Console.Write("Is this correct? (yes / no / cancel): ");
                string? confirm = Console.ReadLine()?.Trim().ToLower();

                if (confirm == "cancel")
                {
                    Console.WriteLine("Add user cancelled.");
                    return;
                }

                if (confirm == "yes")
                    break;

                Console.Write("Enter the number of the field to edit (1-6): ");
                string? fieldChoice = Console.ReadLine()?.Trim();
                switch (fieldChoice)
                {
                    case "1":
                        Console.Write("New First Name: ");
                        firstName = Console.ReadLine()?.Trim() ?? firstName;
                        break;
                    case "2":
                        Console.Write("New Last Name: ");
                        lastName = Console.ReadLine()?.Trim() ?? lastName;
                        break;
                    case "3":
                        Console.Write("New Mother's Maiden Name: ");
                        mothersMaidenName = Console.ReadLine()?.Trim() ?? mothersMaidenName;
                        break;
                    case "4":
                        Console.Write("New Address: ");
                        address = Console.ReadLine()?.Trim() ?? address;
                        break;
                    case "5":
                        Console.Write("New Phone Number: ");
                        phoneNumber = Console.ReadLine()?.Trim() ?? phoneNumber;
                        break;
                    case "6":
                        Console.Write("New Email: ");
                        email = Console.ReadLine()?.Trim() ?? email;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }

            // 5. Insert the new user
            Guid newUserId = Guid.NewGuid();
            const string insertQuery = @"INSERT INTO customer_users
                (customer_user_id, first_name, last_name, mothers_maiden_name, address, phone_number, email)
                VALUES (@id, @firstName, @lastName, @maidenName, @address, @phone, @email)";
            using var insertCmd = new NpgsqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@id", newUserId);
            insertCmd.Parameters.AddWithValue("@firstName", firstName);
            insertCmd.Parameters.AddWithValue("@lastName", lastName);
            insertCmd.Parameters.AddWithValue("@maidenName", mothersMaidenName);
            insertCmd.Parameters.AddWithValue("@address", address);
            insertCmd.Parameters.AddWithValue("@phone", phoneNumber);
            insertCmd.Parameters.AddWithValue("@email", email);
            insertCmd.ExecuteNonQuery();

            // 6. Return the new user's GUID to the admin
            Console.WriteLine($"User added successfully. User ID: {newUserId}");
        }

        public void RemoveUser(NpgsqlConnection conn)
        {
            Console.WriteLine("====== REMOVE USER ======");

            // 1. Collect identifying information
            Console.Write("Email or Phone Number: ");
            string? emailOrPhone = Console.ReadLine()?.Trim();

            Console.Write("Mother's Maiden Name: ");
            string? mothersMaidenName = Console.ReadLine()?.Trim();

            // 2. Validate inputs
            if (string.IsNullOrWhiteSpace(emailOrPhone) || string.IsNullOrWhiteSpace(mothersMaidenName))
            {
                Console.WriteLine("All fields are required. No user was removed.");
                return;
            }

            // 3. Look up the user — must match email/phone AND maiden name
            const string lookupQuery = @"SELECT customer_user_id, first_name, last_name
                                         FROM customer_users
                                         WHERE (email = @emailOrPhone OR phone_number = @emailOrPhone)
                                           AND mothers_maiden_name = @maidenName";
            using var lookupCmd = new NpgsqlCommand(lookupQuery, conn);
            lookupCmd.Parameters.AddWithValue("@emailOrPhone", emailOrPhone);
            lookupCmd.Parameters.AddWithValue("@maidenName", mothersMaidenName);
            using var reader = lookupCmd.ExecuteReader();

            if (!reader.Read())
            {
                Console.WriteLine("No matching user found. No user was removed.");
                return;
            }

            Guid userId = reader.GetGuid(0);
            string fullName = $"{reader.GetString(1)} {reader.GetString(2)}";
            reader.Close();

            // Check that there are no accounts associated with this user before removing
            const string accountCheckQuery = "SELECT COUNT(*) FROM user_accounts WHERE customer_user_id = @id";
            using var accountCheckCmd = new NpgsqlCommand(accountCheckQuery, conn);
            accountCheckCmd.Parameters.AddWithValue("@id", userId);
            long accountCount = (long)(accountCheckCmd.ExecuteScalar() ?? 0L);

            if (accountCount > 0)
            {
                Console.WriteLine($"Cannot remove user. {fullName} has {accountCount} account(s) still associated with them.");
                Console.WriteLine("All accounts must be removed before the user can be deleted.");
                Console.WriteLine("User was not removed.");
                return;
            }

            // 4. Confirm with admin before deleting
            Console.WriteLine($"Found user: {fullName} (ID: {userId})");
            Console.Write("Are you sure you want to remove this user? (yes / no): ");
            string? confirm = Console.ReadLine()?.Trim().ToLower();

            if (confirm != "yes")
            {
                Console.WriteLine("Remove user cancelled.");
                return;
            }

            // 5. Remove the user
            const string deleteQuery = "DELETE FROM customer_users WHERE customer_user_id = @id";
            using var deleteCmd = new NpgsqlCommand(deleteQuery, conn);
            deleteCmd.Parameters.AddWithValue("@id", userId);
            deleteCmd.ExecuteNonQuery();

            Console.WriteLine($"User {fullName} has been removed successfully.");
        }

        public void AddUserAccount(NpgsqlConnection conn)
        {
            Console.WriteLine("====== ADD USER ACCOUNT ======");

            // 1. Collect identifying information to look up the customer
            Console.Write("First Name: ");
            string? firstName = Console.ReadLine()?.Trim();

            Console.Write("Last Name: ");
            string? lastName = Console.ReadLine()?.Trim();

            Console.Write("Mother's Maiden Name: ");
            string? mothersMaidenName = Console.ReadLine()?.Trim();

            Console.Write("Email or Phone Number: ");
            string? emailOrPhone = Console.ReadLine()?.Trim();

            Console.WriteLine("Account Type:");
            Console.WriteLine("  1. Checking");
            Console.WriteLine("  2. Savings");
            Console.Write("> ");
            string? accountTypeInput = Console.ReadLine()?.Trim();
            string accountType = accountTypeInput switch
            {
                "1" => "checking",
                "2" => "savings",
                _ => ""
            };

            // 2. Validate inputs
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(mothersMaidenName) || string.IsNullOrWhiteSpace(emailOrPhone) ||
                string.IsNullOrWhiteSpace(accountType))
            {
                Console.WriteLine("All fields are required and account type must be 1 or 2. No account was created.");
                return;
            }

            // 3. Look up customer in customer_users — must match name, maiden name, and email/phone
            const string lookupQuery = @"SELECT customer_user_id FROM customer_users
                                         WHERE first_name = @firstName
                                           AND last_name = @lastName
                                           AND mothers_maiden_name = @maidenName
                                           AND (email = @emailOrPhone OR phone_number = @emailOrPhone)";
            using var lookupCmd = new NpgsqlCommand(lookupQuery, conn);
            lookupCmd.Parameters.AddWithValue("@firstName", firstName);
            lookupCmd.Parameters.AddWithValue("@lastName", lastName);
            lookupCmd.Parameters.AddWithValue("@maidenName", mothersMaidenName);
            lookupCmd.Parameters.AddWithValue("@emailOrPhone", emailOrPhone);
            using var reader = lookupCmd.ExecuteReader();

            if (!reader.Read())
            {
                Console.WriteLine("No matching customer found. No account was created.");
                return;
            }

            Guid customerId = reader.GetGuid(0);
            reader.Close();

            // 5. Read routing number from environment variables
            var envVars = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines("EnvironmentVariable.txt"))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var parts = line.Split('=', 2);
                if (parts.Length == 2) envVars[parts[0].Trim()] = parts[1].Trim();
            }
            string routingNumber = envVars.TryGetValue("BANK1_ROUTINNUMBER", out var rn) ? rn : string.Empty;

            // Generate a unique 9-digit account number
            string accountNumber;
            bool accountExists;
            do
            {
                accountNumber = Random.Shared.NextInt64(100000000, 999999999).ToString();
                const string checkAccQuery = "SELECT COUNT(*) FROM user_accounts WHERE account_number = @acc";
                using var checkCmd = new NpgsqlCommand(checkAccQuery, conn);
                checkCmd.Parameters.AddWithValue("@acc", accountNumber);
                accountExists = (long)(checkCmd.ExecuteScalar() ?? 0L) > 0;
            } while (accountExists);

            // 6. Ask for optional opening deposit
            Console.Write("Opening deposit amount (enter 0 if none): ");
            string? depositInput = Console.ReadLine()?.Trim();
            if (!decimal.TryParse(depositInput, out decimal initialBalance) || initialBalance < 0)
            {
                Console.WriteLine("Invalid amount. Defaulting to 0.");
                initialBalance = 0;
            }

            // 4. Insert new account into user_accounts
            string accountHolderName = $"{firstName} {lastName}";
            const string insertQuery = @"INSERT INTO user_accounts
                (customer_user_id, account_number, routing_number, account_holder_name, balance, account_type)
                VALUES (@customerId, @accountNumber, @routingNumber, @holderName, @balance, @accountType)";
            using var insertCmd = new NpgsqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@customerId", customerId);
            insertCmd.Parameters.AddWithValue("@accountNumber", accountNumber);
            insertCmd.Parameters.AddWithValue("@routingNumber", routingNumber);
            insertCmd.Parameters.AddWithValue("@holderName", accountHolderName);
            insertCmd.Parameters.AddWithValue("@balance", initialBalance);
            insertCmd.Parameters.AddWithValue("@accountType", accountType);
            insertCmd.ExecuteNonQuery();

            // 8. Return account number and routing number to the admin
            Console.WriteLine($"Account created successfully.");
            Console.WriteLine($"  Account Type:   {accountType}");
            Console.WriteLine($"  Account Number: {accountNumber}");
            Console.WriteLine($"  Routing Number: {routingNumber}");
            Console.WriteLine($"  Opening Balance: {initialBalance:C}");
        }

        public void RemoveUserAccount(NpgsqlConnection conn)
        {
            Console.WriteLine("====== REMOVE USER ACCOUNT ======");

            // 1. Collect identifying information
            Console.Write("Email or Phone Number: ");
            string? emailOrPhone = Console.ReadLine()?.Trim();

            Console.Write("Account Number: ");
            string? accountNumber = Console.ReadLine()?.Trim();

            // 2. Validate inputs
            if (string.IsNullOrWhiteSpace(emailOrPhone) || string.IsNullOrWhiteSpace(accountNumber))
            {
                Console.WriteLine("All fields are required. No account was removed.");
                return;
            }

            // 3. Find the account — must belong to a customer matching the email/phone
            const string lookupQuery = @"SELECT ua.account_number, ua.account_holder_name, ua.balance
                                         FROM user_accounts ua
                                         JOIN customer_users cu ON cu.customer_user_id = ua.customer_user_id
                                         WHERE (cu.email = @emailOrPhone OR cu.phone_number = @emailOrPhone)
                                           AND ua.account_number = @accountNumber";
            using var lookupCmd = new NpgsqlCommand(lookupQuery, conn);
            lookupCmd.Parameters.AddWithValue("@emailOrPhone", emailOrPhone);
            lookupCmd.Parameters.AddWithValue("@accountNumber", accountNumber);
            using var reader = lookupCmd.ExecuteReader();

            if (!reader.Read())
            {
                Console.WriteLine("No matching account found. No account was removed.");
                return;
            }

            string holderName = reader.GetString(1);
            decimal balance = reader.GetDecimal(2);
            reader.Close();

            // 4. Check the account balance is zero before allowing deletion
            if (balance != 0)
            {
                Console.WriteLine($"Cannot remove account. Account {accountNumber} still has a balance of {balance:C}.");
                Console.WriteLine("The balance must be empty before the account can be removed.");
                Console.WriteLine("Account was not removed.");
                return;
            }

            // 5. Confirm with admin before deleting
            Console.WriteLine($"Found account: {accountNumber} (Holder: {holderName})");
            Console.Write("Are you sure you want to remove this account? (yes / no): ");
            string? confirm = Console.ReadLine()?.Trim().ToLower();

            if (confirm != "yes")
            {
                Console.WriteLine("Remove account cancelled.");
                return;
            }

            // 6. Remove the account
            const string deleteQuery = "DELETE FROM user_accounts WHERE account_number = @accountNumber";
            using var deleteCmd = new NpgsqlCommand(deleteQuery, conn);
            deleteCmd.Parameters.AddWithValue("@accountNumber", accountNumber);
            deleteCmd.ExecuteNonQuery();

            Console.WriteLine($"Account {accountNumber} has been removed successfully.");
        }

        public void UpdateUserInfo(NpgsqlConnection conn)
        {
            Console.WriteLine("====== UPDATE USER INFO ======");
            Console.WriteLine("This feature is not yet implemented.");
        }

        public void ViewUserInfo(NpgsqlConnection conn)
        {
            Console.WriteLine("====== VIEW USER INFO ======");
            Console.WriteLine("This feature is not yet implemented.");
        }

        public void FreezeUnfreezeAccount(NpgsqlConnection conn)
        {
            Console.WriteLine("====== FREEZE / UNFREEZE ACCOUNT ======");
            Console.WriteLine("This feature is not yet implemented.");
        }

        public void ViewTransactionHistory(NpgsqlConnection conn)
        {
            Console.WriteLine("====== VIEW TRANSACTION HISTORY ======");
            Console.WriteLine("This feature is not yet implemented.");
        }

        public void ViewPendingTransfers(NpgsqlConnection conn)
        {
            Console.WriteLine("====== VIEW PENDING TRANSFERS ======");
            Console.WriteLine("This feature is not yet implemented.");
        }

        public void CancelPendingTransfer(NpgsqlConnection conn)
        {
            Console.WriteLine("====== CANCEL PENDING TRANSFER ======");
            Console.WriteLine("This feature is not yet implemented.");
        }
    }
}