using Microsoft.AspNetCore.Mvc;
using PaymentNetwork.Models;
using System.Text.Json;

namespace PaymentNetwork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionRequestController : ControllerBase
    {
        // private readonly TransactionService _service;


        // public TransactionRequestController(TransactionService service)
        // {
        //     _service = service;
        // }

        [HttpPost("validateTransaction")]
        public IActionResult InitateTransaction([FromBody] TransactionRequestModel request)
        {
            /*
            ==== to-do ====
            1. payment network should check TABLE banks, which will include all the banks that are within the network
                - proceed if either banks are not within network and return a JSON to the client with the following info; 404 status code, success false, message "bank ____ not supported"
                - proceed if both banks are within network
            2. Network searchs for the customer bank by its routing number and sends request to bank for authorization
            3. Network sends message back to merchant from bank
            4. At end of day, banks will perform fund transfers
            5. API takes each transfer request and routes funds to correct bank
            */
        }
    }
}