namespace eTasks_server.Models.DTOs.DatabaseAdmin.Responses
{
    /// <summary>
    /// Representa uma visão geral do banco de dados, incluindo informações sobre o nome do banco de dados, versão do servidor, contagem de tabelas, número total de linhas, tamanho dos dados e índices, bem como uma lista de resumos das tabelas.
    /// </summary>
    public class DatabaseOverviewResponse
    {
        /// <summary>
        /// Nome do banco de dados.
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Versão do servidor de banco de dados.
        /// </summary>
        public string ServerVersion { get; set; } = string.Empty;

        /// <summary>
        /// Total de tabelas no banco de dados.
        /// </summary>
        public int TableCount { get; set; }

        /// <summary>
        /// Total de linhas em todas as tabelas do banco de dados.
        /// </summary>
        public long TotalRows { get; set; }

        /// <summary>
        /// Tamanho total dos dados em bytes no banco de dados.
        /// </summary>
        public long DataLengthBytes { get; set; }

        /// <summary>
        /// Tamanho total dos índices em bytes no banco de dados.
        /// </summary>
        public long IndexLengthBytes { get; set; }

        /// <summary>
        /// Data e hora em que a visão geral do banco de dados foi gerada.
        /// </summary>
        public DateTime GeneratedAt { get; set; }
                
        /// <summary>
        /// Lista de resumos das tabelas no banco de dados.
        /// </summary>
        public IReadOnlyList<DatabaseTableSummaryResponse> Tables { get; set; } = [];

        /// <summary>
        /// Calcula o tamanho total do banco de dados em bytes, somando o tamanho dos dados e o tamanho dos índices.
        /// </summary>
        public long TotalLengthBytes => DataLengthBytes + IndexLengthBytes;
    }
}
