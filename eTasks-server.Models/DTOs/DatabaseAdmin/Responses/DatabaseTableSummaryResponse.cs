namespace eTasks_server.Models.DTOs.DatabaseAdmin.Responses
{
    /// <summary>
    /// Resumo das informações de uma tabela do banco de dados, incluindo nome, número de linhas, tamanho dos dados e índices, e datas de criação e atualização.
    /// </summary>
    public class DatabaseTableSummaryResponse
    {
        /// <summary>
        /// Nome da tabela no banco de dados.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Número de linhas na tabela.
        /// </summary>
        public long Rows { get; set; }
        
        /// <summary>
        /// Tamanho dos dados da tabela em bytes.
        /// </summary>
        public long DataLengthBytes { get; set; }
        
        /// <summary>
        /// Tamanho dos índices da tabela em bytes.
        /// </summary>
        public long IndexLengthBytes { get; set; }
        
        /// <summary>
        /// Data e hora em que a tabela foi criada.
        /// </summary>
        public DateTime? CreatedAt { get; set; }
        
        /// <summary>
        /// Data e hora em que a tabela foi atualizada.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Calcula o tamanho total da tabela em bytes, somando o tamanho dos dados e o tamanho dos índices.
        /// </summary>
        public long TotalLengthBytes => DataLengthBytes + IndexLengthBytes;
    }
}
