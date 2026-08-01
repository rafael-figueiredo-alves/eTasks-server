using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Core.Services.Models;
using eTasks_server.Models.DTOs.OperationAudit.Requests;
using eTasks_server.Models.DTOs.OperationAudit.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace eTasks_server.Core.BusinessLogicLayers.Admin
{
    /// <summary>
    /// Regras de negocio para consulta e manutencao da auditoria de operacoes em MongoDB.
    /// </summary>
    public class OperationAuditAdminBLL(
        IServerSettingsProvider settingsProvider,
        IConfiguration configuration) : IOperationAuditAdminBLL
    {
        private const int MaxPageSize = 200;

        /// <summary>
        /// Retorna um panorama da auditoria de operacoes.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resumo agregado da auditoria.</returns>
        public async Task<OperationAuditDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            // Monta o contexto da auditoria Mongo a partir das configuracoes atuais.
            var context = await CreateMongoContextAsync(cancellationToken);

            // Preenche o resumo com informacoes basicas, mesmo que a auditoria esteja desabilitada ou incompleta.
            var response = new OperationAuditDashboardResponse
            {
                MongoAuditEnabled = context.Enabled,
                IsConfigured = context.IsConfigured,
                DatabaseName = context.DatabaseName,
                CollectionName = context.CollectionName
            };

            // Se a auditoria estiver desabilitada ou incompleta, devolve o resumo com apenas as informacoes basicas.
            if (context.Collection is null)
            {
                return response;
            }

            // Monta filtros para as consultas agregadas.
            var filter = Builders<OperationAuditLog>.Filter.Empty;
            var last24Filter = Builders<OperationAuditLog>.Filter.Gte(x => x.CreatedAtUtc, DateTime.UtcNow.AddHours(-24));
            var errorFilter = Builders<OperationAuditLog>.Filter.Or(
                Builders<OperationAuditLog>.Filter.Gte(x => x.StatusCode, 500),
                Builders<OperationAuditLog>.Filter.Ne(x => x.ErrorMessage, null));
            var authenticatedFilter = Builders<OperationAuditLog>.Filter.Eq(x => x.IsAuthenticated, true);

            response.TotalEntries = await context.Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            response.EntriesLast24Hours = await context.Collection.CountDocumentsAsync(last24Filter, cancellationToken: cancellationToken);
            response.ErrorEntries = await context.Collection.CountDocumentsAsync(errorFilter, cancellationToken: cancellationToken);
            response.AuthenticatedEntries = await context.Collection.CountDocumentsAsync(authenticatedFilter, cancellationToken: cancellationToken);

            var latest = await context.Collection.Find(filter)
                .SortByDescending(x => x.CreatedAtUtc)
                .Limit(1)
                .FirstOrDefaultAsync(cancellationToken);
            response.LatestEntryAtUtc = latest?.CreatedAtUtc;

            response.AverageDurationMs = await GetAverageDurationAsync(context.Collection, cancellationToken);
            response.StatusCodes = await GetTopMetricsAsync(context.Collection, "StatusCode", 10, cancellationToken);
            response.Methods = await GetTopMetricsAsync(context.Collection, "Method", 10, cancellationToken);
            response.Resources = await GetTopMetricsAsync(context.Collection, "ResourceName", 10, cancellationToken);
            response.UsageTrend = await GetUsageTrendAsync(context.Collection, cancellationToken);

            return response;
        }

        /// <summary>
        /// Retorna uma pagina de entradas da auditoria de operacoes.
        /// </summary>
        /// <param name="request">Parametros de consulta.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Pagina com entradas filtradas.</returns>
        public async Task<OperationAuditLogPageResponse> GetEntriesAsync(OperationAuditLogQueryRequest request, CancellationToken cancellationToken = default)
        {
            var context = await CreateMongoContextAsync(cancellationToken);
            var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
            var pageIndex = Math.Max(0, request.PageIndex);
            var response = new OperationAuditLogPageResponse
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            if (context.Collection is null)
            {
                return response;
            }

            var filter = BuildFilter(request);
            var total = await context.Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            var items = await context.Collection.Find(filter)
                .SortByDescending(x => x.CreatedAtUtc)
                .Skip(pageIndex * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            response.TotalItems = total;
            response.Items = items.Select(MapEntry).ToList();

            return response;
        }

        /// <summary>
        /// Gera um arquivo de backup com as entradas da auditoria.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Arquivo NDJSON com as entradas.</returns>
        public async Task<OperationAuditBackupFileResponse> GenerateBackupAsync(CancellationToken cancellationToken = default)
        {
            var context = await CreateMongoContextAsync(cancellationToken);
            if (context.Collection is null)
            {
                throw new ValidationException("MongoAudit", "Auditoria MongoDB desabilitada ou incompleta.");
            }

            var entries = await context.Collection.Find(Builders<OperationAuditLog>.Filter.Empty)
                .SortBy(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            var builder = new StringBuilder();
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.AppendLine(JsonSerializer.Serialize(MapEntry(entry)));
            }

            return new OperationAuditBackupFileResponse
            {
                FileName = $"{SanitizeFileName(context.DatabaseName)}-{SanitizeFileName(context.CollectionName)}-backup-{DateTime.Now:yyyyMMdd-HHmmss}.ndjson",
                Content = Encoding.UTF8.GetBytes(builder.ToString())
            };
        }

        /// <summary>
        /// Remove todas as entradas da auditoria de operacoes.
        /// </summary>
        /// <param name="adminKey">Chave administrativa de validacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Quantidade de registros removidos.</returns>
        public async Task<long> ClearAsync(string adminKey, CancellationToken cancellationToken = default)
        {
            ValidateAdminKey(adminKey);

            var context = await CreateMongoContextAsync(cancellationToken);
            if (context.Collection is null)
            {
                throw new ValidationException("MongoAudit", "Auditoria MongoDB desabilitada ou incompleta.");
            }

            var result = await context.Collection.DeleteManyAsync(Builders<OperationAuditLog>.Filter.Empty, cancellationToken);
            return result.DeletedCount;
        }

        /// <summary>
        /// Monta o contexto da auditoria Mongo a partir das configuracoes atuais.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Contexto com ou sem acesso ao Mongo.</returns>
        private async Task<MongoAuditContext> CreateMongoContextAsync(CancellationToken cancellationToken)
        {
            var settings = await settingsProvider.GetCurrentAsync(cancellationToken);
            var enabled = settings.MongoAuditEnabled;
            var isConfigured = !string.IsNullOrWhiteSpace(settings.MongoAuditConnectionString)
                && !string.IsNullOrWhiteSpace(settings.MongoAuditDatabaseName)
                && !string.IsNullOrWhiteSpace(settings.MongoAuditCollectionName);

            // Se estiver desabilitado ou incompleto, devolve um contexto sem collection.
            if (!enabled || !isConfigured)
            {
                return new MongoAuditContext(
                    enabled,
                    isConfigured,
                    settings.MongoAuditDatabaseName,
                    settings.MongoAuditCollectionName,
                    null);
            }

            var client = new MongoClient(settings.MongoAuditConnectionString);
            var collection = client
                .GetDatabase(settings.MongoAuditDatabaseName)
                .GetCollection<OperationAuditLog>(settings.MongoAuditCollectionName);

            return new MongoAuditContext(
                enabled,
                isConfigured,
                settings.MongoAuditDatabaseName,
                settings.MongoAuditCollectionName,
                collection);
        }

        /// <summary>
        /// Monta o filtro de consulta da auditoria.
        /// </summary>
        /// <param name="request">Parametros de pesquisa.</param>
        /// <returns>Filtro MongoDB composto.</returns>
        private static FilterDefinition<OperationAuditLog> BuildFilter(OperationAuditLogQueryRequest request)
        {
            var builder = Builders<OperationAuditLog>.Filter;
            var filters = new List<FilterDefinition<OperationAuditLog>>();

            if (!string.IsNullOrWhiteSpace(request.Method))
            {
                filters.Add(builder.Eq(x => x.Method, request.Method.Trim().ToUpperInvariant()));
            }

            if (request.StatusCode.HasValue)
            {
                filters.Add(builder.Eq(x => x.StatusCode, request.StatusCode.Value));
            }

            if (!string.IsNullOrWhiteSpace(request.ResourceName))
            {
                filters.Add(builder.Eq(x => x.ResourceName, request.ResourceName.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var regex = new BsonRegularExpression(RegexEscape(request.Search.Trim()), "i");
                filters.Add(builder.Or(
                    builder.Regex(x => x.Path, regex),
                    builder.Regex(x => x.QueryString, regex),
                    builder.Regex(x => x.EndpointName, regex),
                    builder.Regex(x => x.ResourceName, regex),
                    builder.Regex(x => x.UserAgent, regex),
                    builder.Regex(x => x.IpAddress, regex),
                    builder.Regex(x => x.ErrorMessage, regex),
                    builder.Regex(x => x.TraceIdentifier, regex)));
            }

            return filters.Count == 0 ? builder.Empty : builder.And(filters);
        }

        /// <summary>
        /// Calcula a duracao media dos eventos da auditoria.
        /// </summary>
        /// <param name="collection">Collection Mongo de auditoria.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Duracao media em milissegundos.</returns>
        private static async Task<double> GetAverageDurationAsync(IMongoCollection<OperationAuditLog> collection, CancellationToken cancellationToken)
        {
            var result = await collection.Aggregate()
                .Group(new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "average", new BsonDocument("$avg", "$DurationMs") }
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (result is null || !result.TryGetValue("average", out var average) || average.IsBsonNull)
            {
                return 0;
            }

            return average.ToDouble();
        }

        /// <summary>
        /// Retorna as principais metricas agrupadas por um campo.
        /// </summary>
        /// <param name="collection">Collection Mongo de auditoria.</param>
        /// <param name="fieldName">Nome do campo a agrupar.</param>
        /// <param name="limit">Limite de itens.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de metricas agregadas.</returns>
        private static async Task<IReadOnlyList<OperationAuditMetricResponse>> GetTopMetricsAsync(
            IMongoCollection<OperationAuditLog> collection,
            string fieldName,
            int limit,
            CancellationToken cancellationToken)
        {
            var documents = await collection.Aggregate()
                .Match(new BsonDocument(fieldName, new BsonDocument("$ne", BsonNull.Value)))
                .Group(new BsonDocument
                {
                    { "_id", $"${fieldName}" },
                    { "count", new BsonDocument("$sum", 1) }
                })
                .Sort(new BsonDocument("count", -1))
                .Limit(limit)
                .ToListAsync(cancellationToken);

            return documents
                .Where(document => document.TryGetValue("_id", out var id) && !id.IsBsonNull)
                .Select(document => new OperationAuditMetricResponse
                {
                    Label = document["_id"].ToString() ?? string.Empty,
                    Count = document["count"].ToInt64()
                })
                .ToList();
        }

        /// <summary>
        /// Monta a serie temporal de uso da auditoria.
        /// </summary>
        /// <param name="collection">Collection Mongo de auditoria.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Serie com total e erros por hora.</returns>
        private static async Task<IReadOnlyList<OperationAuditUsagePointResponse>> GetUsageTrendAsync(
            IMongoCollection<OperationAuditLog> collection,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var currentHourUtc = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
            var start = currentHourUtc.AddHours(-24);
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("CreatedAtUtc", new BsonDocument("$gte", start))),
                new BsonDocument("$group", new BsonDocument
                {
                    {
                        "_id",
                        new BsonDocument("$dateToString", new BsonDocument
                        {
                            { "format", "%Y-%m-%d %H:00" },
                            { "date", "$CreatedAtUtc" },
                            { "timezone", "UTC" }
                        })
                    },
                    { "total", new BsonDocument("$sum", 1) },
                    {
                        "errors",
                        new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                        {
                            new BsonDocument("$or", new BsonArray
                            {
                                new BsonDocument("$gte", new BsonArray { "$StatusCode", 400 }),
                                new BsonDocument("$ne", new BsonArray { "$ErrorMessage", BsonNull.Value })
                            }),
                            1,
                            0
                        }))
                    }
                }),
                new BsonDocument("$sort", new BsonDocument("_id", 1))
            };

            var documents = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
            var countsByBucket = documents
                .Select(document =>
                {
                    var label = document["_id"].AsString;
                    var parsed = DateTime.TryParseExact(
                        label,
                        "yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var bucketStartUtc);

                    return new
                    {
                        BucketStartUtc = parsed ? bucketStartUtc : DateTime.MinValue,
                        TotalCount = document["total"].ToInt64(),
                        ErrorCount = document["errors"].ToInt64()
                    };
                })
                .Where(point => point.BucketStartUtc != DateTime.MinValue)
                .ToDictionary(point => point.BucketStartUtc, point => point);

            var trend = new List<OperationAuditUsagePointResponse>();
            for (var bucketStartUtc = start; bucketStartUtc <= currentHourUtc; bucketStartUtc = bucketStartUtc.AddHours(1))
            {
                countsByBucket.TryGetValue(bucketStartUtc, out var point);
                trend.Add(new OperationAuditUsagePointResponse
                {
                    BucketStartUtc = bucketStartUtc,
                    Label = bucketStartUtc.ToLocalTime().ToString("dd/MM HH'h'"),
                    TotalCount = point?.TotalCount ?? 0,
                    ErrorCount = point?.ErrorCount ?? 0
                });
            }

            return trend;
        }

        /// <summary>
        /// Mapeia um documento da auditoria para resposta da API.
        /// </summary>
        /// <param name="entry">Documento da auditoria.</param>
        /// <returns>Resposta mapeada.</returns>
        private static OperationAuditLogEntryResponse MapEntry(OperationAuditLog entry)
            => new()
            {
                Id = entry.Id,
                CreatedAtUtc = entry.CreatedAtUtc,
                TraceIdentifier = entry.TraceIdentifier,
                Method = entry.Method,
                Path = entry.Path,
                QueryString = entry.QueryString,
                EndpointName = entry.EndpointName,
                ResourceName = entry.ResourceName,
                StatusCode = entry.StatusCode,
                DurationMs = entry.DurationMs,
                UserUid = entry.UserUid,
                IsAuthenticated = entry.IsAuthenticated,
                UserAgent = entry.UserAgent,
                IpAddress = entry.IpAddress,
                ErrorMessage = entry.ErrorMessage
            };

        /// <summary>
        /// Escapa caracteres especiais para uso em regex do Mongo.
        /// </summary>
        /// <param name="value">Texto original.</param>
        /// <returns>Texto escapado.</returns>
        private static string RegexEscape(string value)
            => System.Text.RegularExpressions.Regex.Escape(value);

        /// <summary>
        /// Valida a chave administrativa usada em operacoes destrutivas.
        /// </summary>
        /// <param name="adminKey">Chave informada.</param>
        private void ValidateAdminKey(string adminKey)
        {
            var configuredAdminKey = configuration[Constants.AdminApiKeyConfig];
            if (string.IsNullOrWhiteSpace(configuredAdminKey))
            {
                throw new ValidationException(nameof(adminKey), "APIKEY_ADMIN não configurada.");
            }

            // Comparacao em tempo constante para reduzir risco de oracle por timing.
            if (string.IsNullOrWhiteSpace(adminKey)
                || !FixedTimeEquals(adminKey.Trim(), configuredAdminKey.Trim()))
            {
                throw new ValidationException(nameof(adminKey), "Chave administrativa inválida.");
            }
        }

        /// <summary>
        /// Compara duas strings em tempo constante.
        /// </summary>
        /// <param name="value">Valor informado.</param>
        /// <param name="expected">Valor esperado.</param>
        /// <returns>True quando os valores coincidem.</returns>
        private static bool FixedTimeEquals(string value, string expected)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            return valueBytes.Length == expectedBytes.Length
                && CryptographicOperations.FixedTimeEquals(valueBytes, expectedBytes);
        }

        /// <summary>
        /// Sanitiza nome de arquivo para uso em backup.
        /// </summary>
        /// <param name="value">Nome original.</param>
        /// <returns>Nome de arquivo seguro.</returns>
        private static string SanitizeFileName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                builder.Append(invalidChars.Contains(character) ? '-' : character);
            }

            return builder.Length == 0 ? "operation-audit" : builder.ToString();
        }

        private sealed record MongoAuditContext(
            bool Enabled,
            bool IsConfigured,
            string DatabaseName,
            string CollectionName,
            IMongoCollection<OperationAuditLog>? Collection);
    }
}
