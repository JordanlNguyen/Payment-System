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
            var customerAccountInfo = _repository.FindRowsByAccountNumber(request.SourceAccount);
            if (customerAccountInfo == null || customerAccountInfo.Count == 0)
            {
                return JsonSerializer.Serialize(new {statusCode = 404, success = false, message = "no account found", receipt = "none" });
            }
            decimal accountBalance = _repository.GetAccountBalance(request.SourceAccount);
            if (accountBalance < request.Amount)
            {
                return JsonSerializer.Serialize(new {statusCode = 400, success = false, message = "insufficient funds", receipt = "none"});
            }

            // 2. once and if approved, pull money from customer account and put transfer request into a pool of said transfer request for end of day proccessing
            InitateMoneyTransfer(customerAccountInfo[0], request.Amount);
            return JsonSerializer.Serialize(new {statusCode = 200, success = true, message = "transaction approvedl", receipt = new {data = "data", status = "coming soon"}});// receipt should hold date of the transaction, the merchant, the last digits of the customer account, the last digits of the merchant account, and the amount
        }

        public void InitateMoneyTransfer(Dictionary<string, object> customerAccountInfo, decimal amount)
        {
            /*
            - Customer has enough money to withdrawl for the purchase
            - Customer is verified and authorised to complete transaction
            - Payment network already authorised both bank accounts to be within network, ensuring valid currency transfer
           
            ====== Process of Pulling Money from Customer Account ======
            1. get the new balance of account - transaction amount √
            2. update database with new balance √
            to-do. create a table TRANSACTIONS with columns transaction_id, source_account, destination_account, amount, date, status (pending, completed, failed) so that customers can view transactions made
            3. insert transaction data into TRANSACTION table
            ======= Process of putting trasfer request into pool ========
            to-do: create a table TRANSFER_REQUESTS with columns request_id, source_account, destination_account, amount, date, status (pending, completed, failed) so that at the end of the day, the bank can process all transfer requests in the pool and update the status of each transfer request accordingly
            1. Insert request into TRANSFER_REQUESTS table with status pending
            2. seperate script or program will execute at the end of day to process all transfer requests in the TRANSFER_REQUESTS table with status pending, update the status to completed or failed based on the result of the transfer, and update the TRANSACTIONS table with the result of the transfer
            */
            decimal newBalance = (decimal)customerAccountInfo["balance"] - amount;
            _repository.UpdateAccountBalance((string)customerAccountInfo["account_number"], newBalance);

        }
    }
}