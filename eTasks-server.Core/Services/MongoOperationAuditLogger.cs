using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Core.Services.Models;
using eTasks_server.Core.Services.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace eTasks_server.Core.Services
{
    public class MongoOperationAuditLogger : IOperationAuditLogger
    {
        private readonly ILogger<IOperationAuditLogger> _logger;
        private readonly IMongoCollection<OperationAuditLog>? _collection;

        public MongoOperationAuditLogger(IOptions<MongoAuditOptions> options, ILogger<IOperationAuditLogger> logger)
        {
            _logger = logger;
            var settings = options.Value;

            if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                return;
            }

            var client = new MongoClient(settings.ConnectionString);
            _collection = client
                .GetDatabase(settings.DatabaseName)
                .GetCollection<OperationAuditLog>(settings.CollectionName);
        }

        public async Task LogAsync(OperationAuditLog operationLog, CancellationToken cancellationToken = default)
        {
            if (_collection is null)
            {
                return;
            }

            try
            {
                await _collection.InsertOneAsync(operationLog, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao registrar auditoria operacional no MongoDB Atlas. Path {Path}.", operationLog.Path);
            }
        }
    }
}
