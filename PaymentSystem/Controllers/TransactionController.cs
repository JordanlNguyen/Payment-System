using Microsoft.AspNetCore.Mvc;
using PaymentSystem.TransactionRequestModel;
using PaymentSystem.TransactionServices;

namespace PaymentSystem.TransactionController
{
    [ApiController]
    [Route("api/initiateTransaction")]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionServices;

        public TransactionController(TransactionService transactionServices)
        {
            _transactionServices = transactionServices;
        }

        //----
        [HttpPost]
        public IActionResult InitiateTransaction([FromBody] TransactionRequest request)
        {


            /*
            - example JSON request body:
            {
                "merchantId": 123,
                "sourceRoutingNumber": 111000025,
                "sourceAccountNumber": 987654321,
                "destinationRoutingNumber": 222000111,
                "destinationAccountNumber": 123456789,
                "amount": 100.50,
                "date": "2026-04-14T12:00:00Z"
            }

            this endpoint will send true if transaction is approved and false if not
            */


            var result = _transactionServices.ProcessTransaction(request);
            Console.WriteLine($"Transaction result: {result}");
            return Ok(result);
        }
    }
}
