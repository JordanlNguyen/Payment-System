using PaymentSystem.TransactionRequestModel;

namespace PaymentSystem.TransactionServices;
using Npgsql;

public class TransactionService
{
    private readonly NpgsqlConnection _connection;

    public TransactionService(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public bool ProcessTransaction(TransactionRequest transactionRequest)
    {
        /*
        1. search network database to see if customer and merchant banks are within network
        2. once verfied, route request to bank for verfication
        3. send approval or denial back to merchant
        */

        //1
        var result = IsBankInNetwork(transactionRequest);
        //2
        //3
        return true; // Placeholder return value
    }

    public bool IsBankInNetwork(TransactionRequest transactionRequest)
    {
        int MerchantBankRouting = transactionRequest.SourceRoutingNumber;
        int CustomerBankRouting = transactionRequest.DestinationRoutingNumber;

        var conn = _connection;
        conn.Open();
        string sql = "SELECT routingNumber FROM banks WHERE routingNumber = @merchant OR routingNumber = @customer";
        var res = new List<string>();
        using (var cmd = new NpgsqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@merchant", MerchantBankRouting);
            cmd.Parameters.AddWithValue("@customer", CustomerBankRouting);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                res.Add(reader["routingNumber"].ToString());
            }
        }
        conn.Close();
        
        Console.WriteLine("results" + string.Join(", ", res));
        if (res.Count == 2)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }
}