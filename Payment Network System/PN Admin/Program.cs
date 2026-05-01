using System.Globalization;

var app = new AdminCliApp(new InMemoryBankAdminRepository());
return app.Run(args);

internal sealed class AdminCliApp
{
    private readonly IBankAdminRepository _repository;

    public AdminCliApp(IBankAdminRepository repository)
    {
        _repository = repository;
    }

    public int Run(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 0;
        }

        string command = args[0].ToLower(CultureInfo.InvariantCulture);

        return command switch
        {
            "help" => HandleHelp(),
            "list-banks" => HandleListBanks(),
            "add-bank" => HandleAddBank(args),
            "remove-bank" => HandleRemoveBank(args),
            _ => HandleUnknownCommand(command)
        };
    }

    private static int HandleHelp()
    {
        PrintHelp();
        return 0;
    }

    private int HandleListBanks()
    {
        var banks = _repository.GetAllBanks();

        if (banks.Count == 0)
        {
            Console.WriteLine("No banks found.");
            return 0;
        }

        Console.WriteLine("Routing Number | Bank Name | Endpoint URL");
        Console.WriteLine("---------------------------------------------------------------");
        foreach (var bank in banks)
        {
            Console.WriteLine($"{bank.RoutingNumber} | {bank.BankName} | {bank.EndpointUrl}");
        }

        return 0;
    }

    private int HandleAddBank(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: add-bank <routingNumber> <bankName> <endpointUrl>");
            return 1;
        }

        string routingNumber = args[1];
        string bankName = args[2];
        string endpointUrl = args[3];

        if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out _))
        {
            Console.Error.WriteLine("Invalid endpoint URL.");
            return 1;
        }

        bool added = _repository.AddBank(new BankRecord(routingNumber, bankName, endpointUrl));
        if (!added)
        {
            Console.Error.WriteLine("Bank already exists for this routing number.");
            return 1;
        }

        Console.WriteLine("Bank added.");
        return 0;
    }

    private int HandleRemoveBank(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: remove-bank <routingNumber>");
            return 1;
        }

        string routingNumber = args[1];
        bool removed = _repository.RemoveBank(routingNumber);

        if (!removed)
        {
            Console.Error.WriteLine("Bank not found for this routing number.");
            return 1;
        }

        Console.WriteLine("Bank removed.");
        return 0;
    }

    private static int HandleUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Payment Network Admin CLI");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  help");
        Console.WriteLine("      Show help information");
        Console.WriteLine("  list-banks");
        Console.WriteLine("      List all banks in the network");
        Console.WriteLine("  add-bank <routingNumber> <bankName> <endpointUrl>");
        Console.WriteLine("      Add a bank to the network registry");
        Console.WriteLine("  remove-bank <routingNumber>");
        Console.WriteLine("      Remove a bank from the network registry");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  dotnet run -- add-bank 111000025 Chase http://localhost:5058/chaseApi/TransactionRequest/validateTransaction");
    }
}

internal interface IBankAdminRepository
{
    IReadOnlyList<BankRecord> GetAllBanks();
    bool AddBank(BankRecord bank);
    bool RemoveBank(string routingNumber);
}

internal sealed record BankRecord(string RoutingNumber, string BankName, string EndpointUrl);

internal sealed class InMemoryBankAdminRepository : IBankAdminRepository
{
    private readonly Dictionary<string, BankRecord> _banksByRouting = new(StringComparer.Ordinal);

    public IReadOnlyList<BankRecord> GetAllBanks()
    {
        return _banksByRouting.Values
            .OrderBy(x => x.RoutingNumber, StringComparer.Ordinal)
            .ToList();
    }

    public bool AddBank(BankRecord bank)
    {
        if (_banksByRouting.ContainsKey(bank.RoutingNumber))
        {
            return false;
        }

        _banksByRouting[bank.RoutingNumber] = bank;
        return true;
    }

    public bool RemoveBank(string routingNumber)
    {
        return _banksByRouting.Remove(routingNumber);
    }
}