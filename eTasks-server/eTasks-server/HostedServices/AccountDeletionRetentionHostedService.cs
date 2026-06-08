using eTasks_server.Core.Services.Interfaces;

namespace eTasks_server.HostedServices
{
    /// <summary>
    /// Serviço hospedado responsável por aplicar a retenção de contas excluídas, removendo permanentemente aquelas que excederam o período de retenção definido.
    /// </summary>
    /// <param name="scopeFactory">Injeção de serviços</param>
    /// <param name="logger">Instância do logger</param>
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
                    logger.LogWarning(ex, "Falha ao aplicar retenção de contas excluidas.");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
