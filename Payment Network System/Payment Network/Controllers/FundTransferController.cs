using Microsoft.AspNetCore.Mvc;
using PaymentNetwork.Models;
using PaymentNetwork.Services;
using System.Text.Json;


namespace PaymentNetwork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundTransferController : ControllerBase
    {
        private readonly FundTransferService _service;
        public FundTransferController(FundTransferService service)
        {
            _service = service;
        }

        [HttpPost("transferFunds")]
        public Task<IActionResult> initiateFundTransfer([FromBody] FundTransferModel request)
        {
            _service.ProcessFundTransfer(request);
            return Task.FromResult<IActionResult>(Ok(new {statusCode = 200, success = true, message = "fund transfer successful", receipt = "none"}));
        }
    }
}