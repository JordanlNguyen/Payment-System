using PaymentNetwork.Models;
using PaymentNetwork.Repositories;
using System.Text.Json;

namespace PaymentNetwork.Services
{
    public class TransactionRequestService
    {
        private readonly TransactionRequestRepository _repository;
        private readonly HttpClient _httpClient;

        public TransactionRequestService(TransactionRequestRepository repository, HttpClient httpClient)
        {
            _repository = repository;
            _httpClient = httpClient;
        }
        public async Task<string> InitiateTransaction(TransactionRequestModel request)
        {
            // check if both banks are in network
            if (!_repository.AreBothBanksInNetwork(request.SourceRoutingNumber, request.DestinationRoutingNumber))
            {
                return JsonSerializer.Serialize(new {statusCode = 400, success = false, message = "neither bank supported", receipt = "none"});
            }

            // search url of bank by routing number and send request to customer bank
            string url = _repository.SearchForUrl(request.DestinationRoutingNumber);
            var responseFromBank = await _httpClient.PostAsJsonAsync(url, request);
            var content = await responseFromBank.Content.ReadAsStringAsync();

            return content;
        }
    }
}