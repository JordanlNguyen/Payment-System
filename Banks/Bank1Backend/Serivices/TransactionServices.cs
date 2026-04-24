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

        public string ProcessTransaction(TransactionRequestModel request)
        {
            // 1. approve or deny request, message in the form of a JSON will be sent to the merchant so they finish the transaction
            Guid transactionId = Guid.NewGuid();
            var dateTimeNow = DateTime.UtcNow;
            var customerAccountInfo = _repository.FindRowsByAccountNumber(request.SourceAccount);
            if (customerAccountInfo == null || customerAccountInfo.Count == 0)
            {
                //post transaction in transactionhistory table
                return JsonSerializer.Serialize(new {statusCode = 404, success = false, message = "no account found", receipt = "none" });
            }
            decimal accountBalance = _repository.GetAccountBalance(request.SourceAccount);
            if (accountBalance < request.Amount)
            {
                //post transaction in transactionhistory table
                return JsonSerializer.Serialize(new {statusCode = 400, success = false, message = "insufficient funds", receipt = "none"});
            }

            // 2. once and if approved, initiate money transfer procedure and post transaction to customeer account
            InitateMoneyTransfer(transactionId, customerAccountInfo[0], request.Amount, request, dateTimeNow);
            return JsonSerializer.Serialize(new {statusCode = 200, success = true, message = "transaction approved", receipt = new {data = "data", status = "coming soon"}});// receipt should hold date of the transaction, the merchant, the last digits of the customer account, the last digits of the merchant account, and the amount
        }

        public void InitateMoneyTransfer(Guid transactionId, Dictionary<string, object> customerAccountInfo, decimal amount, TransactionRequestModel request, DateTime transactionDate)
        {
            decimal newBalance = (decimal)customerAccountInfo["balance"] - amount;
            _repository.UpdateAccountBalance((string)customerAccountInfo["account_number"], newBalance);
            _repository.PostTransactionInHistory(transactionId, customerAccountInfo, amount, request, transactionDate);
            _repository.UpdateTransferPool(amount);
            _repository.AddTranferPool(request, transactionId, transactionDate);
        }
    }
}