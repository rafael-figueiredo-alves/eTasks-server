using System.Security.Cryptography;
using System.Text;
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
    public class OperationAuditAdminBLL(
        IServerSettingsProvider settingsProvider,
        IConfiguration configuration) : IOperationAuditAdminBLL
    {
        private const int MaxPageSize = 200;

        public async Task<OperationAuditDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            var context = await CreateMongoContextAsync(cancellationToken);
            var response = new OperationAuditDashboardResponse
            {
                MongoAuditEnabled = context.Enabled,
                IsConfigured = context.IsConfigured,
                DatabaseName = context.DatabaseName,
                CollectionName = context.CollectionName
            };

            if (context.Collection is null)
            {
                return response;
            }

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

        private async Task<MongoAuditContext> CreateMongoContextAsync(CancellationToken cancellationToken)
        {
            var settings = await settingsProvider.GetCurrentAsync(cancellationToken);
            var enabled = settings.MongoAuditEnabled;
            var isConfigured = !string.IsNullOrWhiteSpace(settings.MongoAuditConnectionString)
                && !string.IsNullOrWhiteSpace(settings.MongoAuditDatabaseName)
                && !string.IsNullOrWhiteSpace(settings.MongoAuditCollectionName);

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

        private static async Task<IReadOnlyList<OperationAuditUsagePointResponse>> GetUsageTrendAsync(
            IMongoCollection<OperationAuditLog> collection,
            CancellationToken cancellationToken)
        {
            var start = DateTime.UtcNow.AddHours(-24);
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
            return documents.Select(document =>
            {
                var label = document["_id"].AsString;
                _ = DateTime.TryParse(label, out var bucketStart);

                return new OperationAuditUsagePointResponse
                {
                    BucketStartUtc = DateTime.SpecifyKind(bucketStart, DateTimeKind.Utc),
                    Label = bucketStart == default ? label : bucketStart.ToLocalTime().ToString("dd/MM HH'h'"),
                    TotalCount = document["total"].ToInt64(),
                    ErrorCount = document["errors"].ToInt64()
                };
            }).ToList();
        }

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

        private static string RegexEscape(string value)
            => System.Text.RegularExpressions.Regex.Escape(value);

        private void ValidateAdminKey(string adminKey)
        {
            var configuredAdminKey = configuration[Constants.AdminApiKeyConfig];
            if (string.IsNullOrWhiteSpace(configuredAdminKey))
            {
                throw new ValidationException(nameof(adminKey), "APIKEY_ADMIN nao configurada.");
            }

            if (string.IsNullOrWhiteSpace(adminKey)
                || !FixedTimeEquals(adminKey.Trim(), configuredAdminKey.Trim()))
            {
                throw new ValidationException(nameof(adminKey), "Chave administrativa invalida.");
            }
        }

        private static bool FixedTimeEquals(string value, string expected)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            return valueBytes.Length == expectedBytes.Length
                && CryptographicOperations.FixedTimeEquals(valueBytes, expectedBytes);
        }

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
