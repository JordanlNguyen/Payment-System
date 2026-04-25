namespace Bank1Backend.Services
{
    public class TransferFundsSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TransferFundsSchedulerService> _logger;

        public TransferFundsSchedulerService(
            IServiceScopeFactory scopeFactory,
            ILogger<TransferFundsSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                DateTime nextRun = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
                if (now >= nextRun)
                {
                    nextRun = nextRun.AddDays(1);
                }

                TimeSpan delay = nextRun - now;
                _logger.LogInformation("Next transfer settlement run scheduled at {NextRunLocal}", nextRun);

                await Task.Delay(delay, stoppingToken);

                try
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    TransferfundService transferService = scope.ServiceProvider.GetRequiredService<TransferfundService>();

                    transferService.ProcessTransferPool();
                    _logger.LogInformation("Transfer settlement job completed at {RunTimeLocal}", DateTime.Now);
                }
                catch (OperationCanceledException)
                {
                    // Application is shutting down.
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transfer settlement job failed at {RunTimeLocal}", DateTime.Now);
                }
            }
        }
    }
}
