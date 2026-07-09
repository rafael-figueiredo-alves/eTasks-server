namespace eTasks_server.Models.DTOs.GoogleAuth
{
    /// <summary>
    /// Classe de retorno do login com Google
    /// </summary>
    public class GoogleAuthCallbackResult
    {
        /// <summary>
        /// Indicador se houve sucesso
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Código da seção
        /// </summary>
        public Guid? SessionCode { get; set; }

        /// <summary>
        /// Agente do cliente
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// URL de redirecionamento
        /// </summary>
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// Mensagem
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
