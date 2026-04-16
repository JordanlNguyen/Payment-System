namespace PaymentSystem.TransactionRequestModel;
public class TransactionRequest
{
    public int MerchantId { get; set; }
    public int DestinationRoutingNumber { get; set; }
    public int DestinationAccountNumber { get; set; }
    public int SourceRoutingNumber { get; set; }
    public int SourceAccountNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}