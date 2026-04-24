using Microsoft.AspNetCore.Mvc;
using Bank1Backend.Models;
using Bank1Backend.Services;
using System.Text.Json;

namespace Bank1Backend.Controllers
{
    /*
    - [ApiController] specifies that the class is a controller, it is used in c# so that it knows to enable Controller features
    - Route("api/[controller] specifies the route template for the controller, [controller] is a placeholder that will be replaced with the name of the controller (without the "Controller" suffix).
        So in this case, it will be "api/TransactionRequest" for this controller.
    */
    [ApiController]
    [Route("chaseApi/[controller]")]
    public class TransactionRequestController : ControllerBase
    {
        private readonly TransactionService _service;

        public TransactionRequestController(TransactionService service)
        {
            _service = service;
        }

        // endpoint to recieve transation request and send true or false if the transaction is approved
        [HttpPost("validateTransaction")]
        public IActionResult InitiateTransaction([FromBody] TransactionRequestModel request)
        {
            var res = _service.ProcessTransaction(request);
            using var doc = JsonDocument.Parse(res);
            var status = doc.RootElement.GetProperty("statusCode").GetInt32();
            return new ContentResult
            {
                Content = res,
                ContentType = "application/json",
                StatusCode = status
            };
        }
    }
}