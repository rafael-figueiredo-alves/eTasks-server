using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Core.Services.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace eTasks_server.Core.Services
{
    public class MongoOperationAuditLogger : IOperationAuditLogger
    {
        private readonly ILogger<IOperationAuditLogger> _logger;
        private readonly IServerSettingsProvider _settingsProvider;

        public MongoOperationAuditLogger(IServerSettingsProvider settingsProvider, ILogger<IOperationAuditLogger> logger)
        {
            _settingsProvider = settingsProvider;
            _logger = logger;
        }

        public async Task LogAsync(OperationAuditLog operationLog, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsProvider.GetCurrentAsync(cancellationToken);
            if (!settings.MongoAuditEnabled || string.IsNullOrWhiteSpace(settings.MongoAuditConnectionString))
            {
                return;
            }

            try
            {
                var client = new MongoClient(settings.MongoAuditConnectionString);
                var collection = client
                    .GetDatabase(settings.MongoAuditDatabaseName)
                    .GetCollection<OperationAuditLog>(settings.MongoAuditCollectionName);

                await collection.InsertOneAsync(operationLog, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao registrar auditoria operacional no MongoDB Atlas. Path {Path}.", operationLog.Path);
            }
        }
    }
}
