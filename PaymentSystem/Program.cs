using Npgsql;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register TransactionService for DI
builder.Services.AddScoped<PaymentSystem.TransactionServices.TransactionService>();

//making database connections
var envPath = Path.Combine(builder.Environment.ContentRootPath, "EnvironmentVariables.txt");
if (!File.Exists(envPath))
{
    throw new FileNotFoundException("EnvironmentVariables.txt was not found.", envPath);
}

var envLines = File.ReadAllLines(envPath)
    .Select(line => line.Trim())
    .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("#"))
    .Select(line => line.Split('=', 2))
    .Where(parts => parts.Length == 2)
    .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());

var connectionStringBuilder = new NpgsqlConnectionStringBuilder
{
    Host = envLines.GetValueOrDefault("DB_HOST", "127.0.0.1"),
    Port = int.Parse(envLines.GetValueOrDefault("DB_PORT", "5432")),
    Database = envLines.GetValueOrDefault("DB_NAME", "paymentsystem"),
    Username = envLines.GetValueOrDefault("DB_USER", "postgres"),
    Password = envLines.GetValueOrDefault("DB_PASSWORD", string.Empty)
};

builder.Services.AddSingleton(new NpgsqlConnection(connectionStringBuilder.ConnectionString));

var app = builder.Build();


// Main endpoint to display 'Payment System'
app.MapGet("/", () => "Payment System");

app.MapControllers();

app.Run();

// file routes http request to the controller directory. It does this by scanning the project for any endpoints