namespace eTasks_server.Models.DTOs.DatabaseAdmin.Responses
{
    /// <summary>
    /// Resposta a execução de um script SQL no banco de dados.
    /// </summary>
    public class DatabaseScriptExecutionResponse
    {
        /// <summary>
        /// Indica se a execução do script foi bem-sucedida.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Número de linhas afetadas pela execução do script.
        /// </summary>
        public int AffectedRows { get; set; }

        /// <summary>
        /// Mensagem detalhando o resultado da execução do script, incluindo erros ou informações adicionais.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora em que o script foi executado.
        /// </summary>
        public DateTime ExecutedAt { get; set; }
    }
}
