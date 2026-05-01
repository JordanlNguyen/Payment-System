using PaymentNetwork.Models;

namespace PaymentNetwork.Services
{
    public class FundTransferService
    {
        public void ProcessFundTransfer(FundTransferModel request)
        {
            /*
            1. search the database for the destination bank url to deposit fund
            2. use url to deposit funds into destination bank
            */
        }
    }
}