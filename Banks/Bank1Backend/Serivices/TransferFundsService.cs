using Bank1Backend.Repositories;

namespace Bank1Backend.Services
{
    public class TransferfundService
    {
        private readonly TransactionRepository _repository;
        public TransferfundService(TransactionRepository repository)
        {
            _repository = repository;
        }

        public void ProcessTransferPool()
        {
            /*
            === function should initiate transfer of funds for that day ===
                1. query all transfer request rows with status pending
                2. for each request, attempt to transfer through the payment network
                3. once transfer succeeds, update request status to completed and update transaction history
                4. if transfer fails, mark the request failed and keep failure reason for retry/inspection
            */

            var pendingTransferRows = _repository.GetPendingTransferPoolRows();

            // TODO: for each row, determine format the info (destination account, destination routing, amount of funds, mercahnt name, and merchant id) and send to network. for transfer.
            foreach (var row in pendingTransferRows)
            {
                //
            }
        }
    }
}