namespace eTasks_server.Models.DTOs.DatabaseAdmin.Responses
{
    /// <summary>
    /// Classe que representa a resposta de um arquivo de backup do banco de dados.
    /// </summary>
    public class DatabaseBackupFileResponse
    {
        /// <summary>
        /// Nome do arquivo de backup do banco de dados.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de conteúdo do arquivo de backup do banco de dados.
        /// </summary>
        public string ContentType { get; set; } = "application/sql";

        /// <summary>
        /// Conteúdo do arquivo de backup do banco de dados em formato de array de bytes.
        /// </summary>
        public byte[] Content { get; set; } = [];
    }
}
