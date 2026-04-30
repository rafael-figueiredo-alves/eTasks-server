using eTasks_server.Core.Services.Interfaces;

namespace eTasks_server.HostedServices
{
    public class ApplicationLogRetentionHostedService(IServiceScopeFactory scopeFactory, ILogger<ApplicationLogRetentionHostedService> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IApplicationLogRetentionService>();
                    var deleted = await service.ApplyRetentionAsync(stoppingToken);
                    if (deleted > 0)
                    {
                        logger.LogInformation("{DeletedCount} arquivo(s) de log expirado(s) removido(s).", deleted);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Falha ao aplicar retencao de logs da aplicacao.");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
