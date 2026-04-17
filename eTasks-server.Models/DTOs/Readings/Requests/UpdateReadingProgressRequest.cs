namespace eTasks_server.Models.DTOs.Readings.Requests
{
    /// <summary>
    /// Dados para atualizar somente o progresso da leitura.
    /// </summary>
    public class UpdateReadingProgressRequest
    {
        /// <summary>
        /// Página atual da leitura.
        /// </summary>
        public int CurrentPage { get; set; }
    }
}
