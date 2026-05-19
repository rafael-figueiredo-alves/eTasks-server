using eTasks_server.Core.Services.Interfaces;

namespace eTasks_server.HostedServices
{
    public class AccountDeletionRetentionHostedService(IServiceScopeFactory scopeFactory, ILogger<AccountDeletionRetentionHostedService> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IAccountDeletionRetentionService>();
                    var deleted = await service.DeleteExpiredAccountsAsync(stoppingToken);
                    if (deleted > 0)
                    {
                        logger.LogInformation("{DeletedCount} conta(s) expirada(s) removida(s) permanentemente.", deleted);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Falha ao aplicar retencao de contas excluidas.");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
