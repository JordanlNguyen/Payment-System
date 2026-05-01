using Microsoft.AspNetCore.Mvc;
using PaymentNetwork.Models;
using PaymentNetwork.Services;
using System.Text.Json;

namespace PaymentNetwork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionRequestController : ControllerBase
    {
        private readonly TransactionRequestService _service;
        public TransactionRequestController(TransactionRequestService service)
        {
            _service = service;
        }
        
        [HttpPost("validateTransaction")]
        public async Task<IActionResult> InitateTransaction([FromBody] TransactionRequestModel request)
        {
            // send transaction request to service for prpcessing and will return the response from the receiving bank as is to the client
            var result = await _service.InitiateTransaction(request);
            using var doc = JsonDocument.Parse(result);
            int statusCode = doc.RootElement.GetProperty("statusCode").GetInt32();
            return new ContentResult
            {
                Content = result,
                ContentType = "application/json",
                StatusCode = statusCode
            };
        }
    }
}