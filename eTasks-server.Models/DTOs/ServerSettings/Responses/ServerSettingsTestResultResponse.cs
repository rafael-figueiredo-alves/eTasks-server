namespace eTasks_server.Models.DTOs.ServerSettings.Responses
{
    /// <summary>
    /// Resultado do teste dos serviços das configurações
    /// </summary>
    public class ServerSettingsTestResultResponse
    {
        /// <summary>
        /// Indica se houve sucesso
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem de retorno
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
