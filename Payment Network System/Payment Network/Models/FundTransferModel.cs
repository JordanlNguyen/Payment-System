namespace PaymentNetwork.Models
{
    public class FundTransferModel
    {
        public required string SourceRoutingNumber { get; set; }
        public required string SourceAccount { get; set; }
        public required string DestinationRoutingNumber { get; set; }
        public required string DestinationAccount { get; set; }
        public decimal Amount { get; set; }
    }
}