using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.DatabaseAdmin.Requests;
using eTasks_server.Models.DTOs.DatabaseAdmin.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.Admin
{
    /// <summary>
    /// Regras de negocio para operacoes administrativas sobre o banco de dados.
    /// </summary>
    public partial class DatabaseAdminBLL : BaseBLL<IDatabaseAdminBLL>, IDatabaseAdminBLL
    {
        private const int MaxScriptLength = 200_000;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa uma nova instancia do DatabaseAdminBLL.
        /// </summary>
        /// <param name="context">Contexto de dados</param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuracao</param>
        public DatabaseAdminBLL(AppDbContext context, ILogger<IDatabaseAdminBLL> logger, IConfiguration configuration)
            : base(context, logger)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Retorna um panorama do banco de dados atual.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resumo das tabelas e do banco.</returns>
        public async Task<DatabaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = await OpenIfNeededAsync(connection, cancellationToken);

            try
            {
                var databaseName = Convert.ToString(await ExecuteScalarAsync(connection, "SELECT DATABASE();", cancellationToken)) ?? string.Empty;
                var serverVersion = Convert.ToString(await ExecuteScalarAsync(connection, "SELECT VERSION();", cancellationToken)) ?? string.Empty;
                var tables = await LoadTableSummariesAsync(connection, cancellationToken);

                return new DatabaseOverviewResponse
                {
                    DatabaseName = databaseName,
                    ServerVersion = serverVersion,
                    TableCount = tables.Count,
                    TotalRows = tables.Sum(x => x.Rows),
                    DataLengthBytes = tables.Sum(x => x.DataLengthBytes),
                    IndexLengthBytes = tables.Sum(x => x.IndexLengthBytes),
                    GeneratedAt = SaoPauloDateTime.Now(),
                    Tables = tables
                };
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }
        }

        /// <summary>
        /// Gera um arquivo de backup SQL da base atual.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Arquivo de backup gerado.</returns>
        public async Task<DatabaseBackupFileResponse> GenerateBackupAsync(CancellationToken cancellationToken = default)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = await OpenIfNeededAsync(connection, cancellationToken);

            try
            {
                var databaseName = Convert.ToString(await ExecuteScalarAsync(connection, "SELECT DATABASE();", cancellationToken)) ?? "database";
                var tables = await LoadTableSummariesAsync(connection, cancellationToken);
                var script = new StringBuilder();

                script.AppendLine("-- eTasks-server database backup");
                script.AppendLine($"-- Database: {databaseName}");
                script.AppendLine($"-- Generated at: {SaoPauloDateTime.Now():yyyy-MM-dd HH:mm:ss}");
                script.AppendLine();
                script.AppendLine("SET FOREIGN_KEY_CHECKS=0;");
                script.AppendLine();

                foreach (var table in tables.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var tableName = QuoteIdentifier(table.Name);
                    var createStatement = await GetCreateTableStatementAsync(connection, table.Name, cancellationToken);

                    script.AppendLine($"DROP TABLE IF EXISTS {tableName};");
                    script.AppendLine($"{createStatement};");
                    script.AppendLine();

                    await AppendTableRowsAsync(connection, table.Name, script, cancellationToken);
                    script.AppendLine();
                }

                script.AppendLine("SET FOREIGN_KEY_CHECKS=1;");

                return new DatabaseBackupFileResponse
                {
                    FileName = $"{SanitizeFileName(databaseName)}-backup-{DateTime.Now:yyyyMMdd-HHmmss}.sql",
                    ContentType = "application/sql",
                    Content = Encoding.UTF8.GetBytes(script.ToString())
                };
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }
        }

        /// <summary>
        /// Executa um script SQL informado pelo administrador.
        /// </summary>
        /// <param name="request">Conteudo do script SQL.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resultado da execucao do script.</returns>
        public async Task<DatabaseScriptExecutionResponse> ExecuteScriptAsync(DatabaseScriptExecutionRequest request, CancellationToken cancellationToken = default)
        {
            ValidateScript(request.Script);

            var affectedRows = await _context.Database.ExecuteSqlRawAsync(request.Script, cancellationToken);

            return new DatabaseScriptExecutionResponse
            {
                Success = true,
                AffectedRows = affectedRows,
                Message = $"Script executado com sucesso. Linhas afetadas: {affectedRows}.",
                ExecutedAt = SaoPauloDateTime.Now()
            };
        }

        /// <summary>
        /// Limpa a base de dados preservando contas administrativas.
        /// </summary>
        /// <param name="adminKey">Chave administrativa de validacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resultado da limpeza da base.</returns>
        public async Task<DatabaseScriptExecutionResponse> ClearDatabaseAsync(string adminKey, CancellationToken cancellationToken = default)
        {
            ValidateAdminKey(adminKey);

            var connection = _context.Database.GetDbConnection();
            var shouldClose = await OpenIfNeededAsync(connection, cancellationToken);

            try
            {
                var tables = await LoadTableSummariesAsync(connection, cancellationToken);
                var affectedRows = 0;

                await ExecuteNonQueryAsync(connection, "SET FOREIGN_KEY_CHECKS=0;", cancellationToken);

                foreach (var table in tables.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (table.Name.Equals("__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase)
                        || table.Name.Equals("users", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    affectedRows += await ExecuteNonQueryAsync(connection, $"DELETE FROM {QuoteIdentifier(table.Name)};", cancellationToken);
                }

                affectedRows += await ExecuteNonQueryAsync(connection, "DELETE FROM `users` WHERE `IsAdmin` = 0;", cancellationToken);
                await ExecuteNonQueryAsync(connection, "SET FOREIGN_KEY_CHECKS=1;", cancellationToken);

                _logger.LogWarning("Base MySQL limpa pelo painel administrativo. Usuários administradores foram preservados.");

                return new DatabaseScriptExecutionResponse
                {
                    Success = true,
                    AffectedRows = affectedRows,
                    Message = $"Base limpa com sucesso. Linhas removidas: {affectedRows}. Usuários administradores preservados.",
                    ExecutedAt = SaoPauloDateTime.Now()
                };
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    await ExecuteNonQueryAsync(connection, "SET FOREIGN_KEY_CHECKS=1;", cancellationToken);
                }

                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }
        }

        /// <summary>
        /// Carrega o resumo das tabelas do banco.
        /// </summary>
        /// <param name="connection">Conexao aberta com o banco.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de tabelas com metadados.</returns>
        private static async Task<List<DatabaseTableSummaryResponse>> LoadTableSummariesAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT TABLE_NAME, COALESCE(DATA_LENGTH, 0), COALESCE(INDEX_LENGTH, 0), CREATE_TIME, UPDATE_TIME
                FROM information_schema.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                ORDER BY TABLE_NAME;
                """;

            var tables = new List<DatabaseTableSummaryResponse>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add(new DatabaseTableSummaryResponse
                {
                    Name = reader.GetString(0),
                    DataLengthBytes = Convert.ToInt64(reader.GetValue(1), CultureInfo.InvariantCulture),
                    IndexLengthBytes = Convert.ToInt64(reader.GetValue(2), CultureInfo.InvariantCulture),
                    CreatedAt = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    UpdatedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
                });
            }

            await reader.CloseAsync();

            // Busca a contagem real de linhas depois da leitura dos metadados.
            foreach (var table in tables)
            {
                table.Rows = Convert.ToInt64(
                    await ExecuteScalarAsync(connection, $"SELECT COUNT(*) FROM {QuoteIdentifier(table.Name)};", cancellationToken),
                    CultureInfo.InvariantCulture);
            }

            return tables;
        }

        /// <summary>
        /// Obtém o comando CREATE TABLE de uma tabela.
        /// </summary>
        /// <param name="connection">Conexao aberta com o banco.</param>
        /// <param name="tableName">Nome da tabela.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>SQL do CREATE TABLE.</returns>
        private static async Task<string> GetCreateTableStatementAsync(DbConnection connection, string tableName, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SHOW CREATE TABLE {QuoteIdentifier(tableName)};";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return reader.GetString(1);
            }

            throw new InvalidOperationException($"Não foi possível gerar o CREATE TABLE para {tableName}.");
        }

        /// <summary>
        /// Acrescenta ao script os INSERTs de uma tabela.
        /// </summary>
        /// <param name="connection">Conexao aberta com o banco.</param>
        /// <param name="tableName">Nome da tabela.</param>
        /// <param name="script">Builder do script SQL.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        private static async Task AppendTableRowsAsync(DbConnection connection, string tableName, StringBuilder script, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {QuoteIdentifier(tableName)};";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!reader.HasRows)
            {
                return;
            }

            var columns = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetName)
                .Select(QuoteIdentifier)
                .ToArray();

            while (await reader.ReadAsync(cancellationToken))
            {
                var values = new string[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    values[i] = ToSqlLiteral(reader.GetValue(i));
                }

                script.Append("INSERT INTO ")
                    .Append(QuoteIdentifier(tableName))
                    .Append(" (")
                    .AppendJoin(", ", columns)
                    .Append(") VALUES (")
                    .AppendJoin(", ", values)
                    .AppendLine(");");
            }
        }

        /// <summary>
        /// Executa um comando escalar no banco.
        /// </summary>
        /// <param name="connection">Conexao aberta com o banco.</param>
        /// <param name="sql">SQL a executar.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Valor escalar retornado.</returns>
        private static async Task<object?> ExecuteScalarAsync(DbConnection connection, string sql, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            return await command.ExecuteScalarAsync(cancellationToken);
        }

        /// <summary>
        /// Executa um comando sem retorno de linhas.
        /// </summary>
        /// <param name="connection">Conexao aberta com o banco.</param>
        /// <param name="sql">SQL a executar.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Quantidade de linhas afetadas.</returns>
        private static async Task<int> ExecuteNonQueryAsync(DbConnection connection, string sql, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Abre a conexao apenas quando necessario.
        /// </summary>
        /// <param name="connection">Conexao com o banco.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>True quando a funcao abriu a conexao.</returns>
        private static async Task<bool> OpenIfNeededAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            if (connection.State == ConnectionState.Open)
            {
                return false;
            }

            await connection.OpenAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Valida se o script SQL pode ser executado pela tela administrativa.
        /// </summary>
        /// <param name="script">Script SQL informado.</param>
        private static void ValidateScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                throw new ValidationException(nameof(script), "Informe o script SQL.");
            }

            // Limite de tamanho para evitar scripts abusivos na interface.
            if (script.Length > MaxScriptLength)
            {
                throw new ValidationException(nameof(script), $"O script deve ter no máximo {MaxScriptLength} caracteres.");
            }

            // Remove comentarios e literais antes de procurar comandos bloqueados.
            var normalized = StripAllowedForeignKeyActions(StripSqlStringLiterals(StripSqlComments(script)));
            if (BlockedCommandRegex().IsMatch(normalized))
            {
                throw new ValidationException(nameof(script), "Comandos destrutivos ou administrativos não são permitidos nesta tela.");
            }
        }

        /// <summary>
        /// Remove comentarios de SQL do script informado.
        /// </summary>
        /// <param name="script">Script SQL.</param>
        /// <returns>Script sem comentarios.</returns>
        private static string StripSqlComments(string script)
        {
            var withoutBlockComments = Regex.Replace(script, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            var withoutLineComments = Regex.Replace(withoutBlockComments, @"(^|\s)(--|#).*$", " ", RegexOptions.Multiline);
            return withoutLineComments;
        }

        /// <summary>
        /// Remove literais de string para evitar falsos positivos na validacao do script.
        /// </summary>
        /// <param name="script">Script SQL.</param>
        /// <returns>Script sem literais.</returns>
        private static string StripSqlStringLiterals(string script)
            => Regex.Replace(script, @"'(''|\\.|[^'])*'", "''", RegexOptions.Singleline);

        /// <summary>
        /// Mantem apenas as acoes de FK permitidas durante a validacao do script.
        /// </summary>
        /// <param name="script">Script SQL.</param>
        /// <returns>Script filtrado para validacao.</returns>
        private static string StripAllowedForeignKeyActions(string script)
            => Regex.Replace(script, @"\bON\s+DELETE\s+(CASCADE|SET\s+NULL|RESTRICT|NO\s+ACTION)\b", " ", RegexOptions.IgnoreCase);

        /// <summary>
        /// Coloca um identificador SQL entre crases.
        /// </summary>
        /// <param name="identifier">Identificador da tabela ou coluna.</param>
        /// <returns>Identificador quoted.</returns>
        private static string QuoteIdentifier(string identifier)
            => $"`{identifier.Replace("`", "``", StringComparison.Ordinal)}`";

        /// <summary>
        /// Converte um valor CLR para literal SQL.
        /// </summary>
        /// <param name="value">Valor de entrada.</param>
        /// <returns>Literal SQL seguro.</returns>
        private static string ToSqlLiteral(object value)
        {
            if (value is null || value == DBNull.Value)
            {
                return "NULL";
            }

            return value switch
            {
                byte[] bytes => $"X'{Convert.ToHexString(bytes)}'",
                bool boolean => boolean ? "1" : "0",
                DateTime dateTime => $"'{dateTime:yyyy-MM-dd HH:mm:ss.ffffff}'",
                DateTimeOffset dateTimeOffset => $"'{dateTimeOffset:yyyy-MM-dd HH:mm:ss.ffffff zzz}'",
                Guid guid => $"'{guid}'",
                string text => $"'{EscapeSqlString(text)}'",
                char character => $"'{EscapeSqlString(character.ToString())}'",
                TimeSpan timeSpan => $"'{timeSpan}'",
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? "NULL",
                _ => $"'{EscapeSqlString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)}'"
            };
        }

        /// <summary>
        /// Escapa caracteres especiais de uma string SQL.
        /// </summary>
        /// <param name="value">Texto original.</param>
        /// <returns>Texto escapado.</returns>
        private static string EscapeSqlString(string value)
            => value.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("'", "''", StringComparison.Ordinal)
                .Replace("\0", "\\0", StringComparison.Ordinal);

        /// <summary>
        /// Normaliza um nome de arquivo removendo caracteres invalidos.
        /// </summary>
        /// <param name="value">Nome original.</param>
        /// <returns>Nome sanitizado.</returns>
        private static string SanitizeFileName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                builder.Append(invalidChars.Contains(character) ? '-' : character);
            }

            return builder.Length == 0 ? "database" : builder.ToString();
        }

        /// <summary>
        /// Valida a chave administrativa configurada para operacoes destrutivas.
        /// </summary>
        /// <param name="adminKey">Chave informada.</param>
        private void ValidateAdminKey(string adminKey)
        {
            var configuredAdminKey = _configuration[Constants.AdminApiKeyConfig];
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
        /// Compara duas strings com tempo constante.
        /// </summary>
        /// <param name="value">Valor informado.</param>
        /// <param name="expected">Valor esperado.</param>
        /// <returns>True quando os valores forem iguais.</returns>
        private static bool FixedTimeEquals(string value, string expected)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            return valueBytes.Length == expectedBytes.Length
                && CryptographicOperations.FixedTimeEquals(valueBytes, expectedBytes);
        }

        [GeneratedRegex(@"\b(DROP|TRUNCATE|DELETE|RENAME|GRANT|REVOKE|CREATE\s+USER|ALTER\s+USER|CREATE\s+DATABASE|ALTER\s+DATABASE|USE|SET\s+PASSWORD|SHUTDOWN|KILL|LOAD\s+DATA|LOCK\s+TABLES|UNLOCK\s+TABLES|INTO\s+OUTFILE|INTO\s+DUMPFILE)\b", RegexOptions.IgnoreCase)]
        private static partial Regex BlockedCommandRegex();
    }
}
