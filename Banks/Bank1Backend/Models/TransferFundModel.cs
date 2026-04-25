namespace Bank1Backend.Models
{
    public class TransferFundModel
    {
        public required string DestinationAccount { get; set; }
        public required string DestinationRouting { get; set; }
        public required decimal Amount { get; set; }
        public required string MerchantName { get; set; }
        public required string MerchantId { get; set; }
    }
}