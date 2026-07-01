namespace eTasks_server.Models.DTOs.DatabaseAdmin.Requests
{
    /// <summary>
    /// Classe que representa uma solicitação para a execução de um script SQL no banco de dados.
    /// </summary>
    public class DatabaseScriptExecutionRequest
    {
        /// <summary>
        /// Script SQL a ser executado no banco de dados.
        /// </summary>
        public string Script { get; set; } = string.Empty;
    }
}
