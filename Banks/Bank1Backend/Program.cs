using Banks;
using Bank1Backend.Repositories;
using Bank1Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Read environment variables from file and create Bank object
var envVars = new System.Collections.Generic.Dictionary<string, string>();
var lines = System.IO.File.ReadAllLines("EnvironmentVariables.txt");
foreach (var line in lines)
{
    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
    var parts = line.Split('=', 2);
    if (parts.Length == 2)
        envVars[parts[0].Trim()] = parts[1].Trim();
}

// Reading 
string bankName = envVars.ContainsKey("BANK1_NAME") ? envVars["BANK1_NAME"] : "";
string address = envVars.ContainsKey("BANK1_ADDRESS") ? envVars["BANK1_ADDRESS"] : "";
string phoneNumber = envVars.ContainsKey("BANK1_PHONENUMBER") ? envVars["BANK1_PHONENUMBER"] : "";
string routingNumber = envVars.ContainsKey("BANK1_ROUTINGNUMBER") ? envVars["BANK1_ROUTINGNUMBER"] : "";

// Build connection string from environment variables
string dbHost = envVars.ContainsKey("DB_HOST") ? envVars["DB_HOST"] : "";
string dbPort = envVars.ContainsKey("DB_PORT") ? envVars["DB_PORT"] : "";
string dbUser = envVars.ContainsKey("DB_USERNAME") ? envVars["DB_USERNAME"] : "";
string dbPass = envVars.ContainsKey("DB_PASSWORD") ? envVars["DB_PASSWORD"] : "";
string dbName = envVars.ContainsKey("DB_NAME") ? envVars["DB_NAME"] : "";
string connectionString = $"Host={dbHost};Port={dbPort};Username={dbUser};Password={dbPass};Database={dbName}";

// this line of code creates a bank object for bank1 and the connection string for the correct database associated with bank1
Bank bank = new Bank(bankName, address, phoneNumber, routingNumber, connectionString);

//registering singleton and add inject Controllers and Services
builder.Services.AddSingleton(bank);
builder.Services.AddScoped<TransactionRepository>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<TransferfundService>(); //automatically starts backend service
builder.Services.AddHostedService<TransferFundsSchedulerService>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => $"Welcome to {bank.Name} Bank API! Location at {bank.Address} and phone number {bank.PhoneNumber}");
app.MapControllers();

app.Run();
