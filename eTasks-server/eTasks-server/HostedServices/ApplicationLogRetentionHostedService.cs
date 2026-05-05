using eTasks_server.Core.Services.Interfaces;

namespace eTasks_server.HostedServices
{
    /// <summary>
    /// Serviço background para aplicar a retenção de logs da aplicação, removendo arquivos de log expirados periodicamente.
    /// </summary>
    /// <param name="scopeFactory">Fábrica de escopos para resolver dependências</param>
    /// <param name="logger">Logger para registrar informações e erros</param>
    public class ApplicationLogRetentionHostedService(IServiceScopeFactory scopeFactory, ILogger<ApplicationLogRetentionHostedService> logger)
        : BackgroundService
    {
        /// <summary>
        /// Método principal do serviço, executado em loop até que o serviço seja cancelado. Ele resolve o serviço de retenção de logs e aplica a retenção, removendo arquivos expirados. O processo é repetido a cada 12 horas.
        /// </summary>
        /// <param name="stoppingToken">Token para cancelar a operação</param>
        /// <returns>Task representando a operação assíncrona</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //Pegando contexto de serviços injetados para resolver o serviço de retenção de logs
                    using var scope = scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IApplicationLogRetentionService>();

                    //Aplicando a retenção de logs e removendo arquivos expirados
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
                    logger.LogWarning(ex, "Falha ao aplicar retenção de logs da aplicação.");
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
