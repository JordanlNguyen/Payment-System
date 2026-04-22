using Bank1Backend.Models;
using Bank1Backend.Repositories;
using System.Text.Json;

namespace Bank1Backend.Services
{
    public class TransactionService
    {
        private readonly TransactionRepository _repository;

        public TransactionService(TransactionRepository repository)
        {
            _repository = repository;
        }

        public string ValidateTransaction(TransactionRequestModel request)
        {
            /*
            1. Search account according to the account number
            2. Make sure that customer has enough money in its account for the purchase
            3. Perfor some AI modeling procesdure to ensure no fraudulant charges
            4. Send approve or disapprove
            */
            /*
            if account is empty, return False(meaning there are no accounts found)
            if accoount is not empty, then check if account has enough money, if not return False, if it has enough money, then perform AI modeling procedure to check for fraud, if it is fraud, return False, if it is not fraud, return True
            */
            var accountInfo = _repository.FindRowsByAccountNumber(request.SourceAccount);
            if (accountInfo == null || accountInfo.Count == 0)
            {
                return JsonSerializer.Serialize(new {statusCode = 404, success = false, message = "no account found" });
            }
            else
            {
                decimal accountBalance = _repository.GetAccountBalance(request.SourceAccount);
                if (accountBalance < request.Amount)
                {
                    return JsonSerializer.Serialize(new {statusCode = 400, success = false, message = "insufficient funds" });
                }
                else
                {
                    // perform AI fraud check
                    return JsonSerializer.Serialize(new {statusCode = 200, success = true, message = "transaction approved" });
                }
            }

        }
    }
}