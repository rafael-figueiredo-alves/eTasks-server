namespace eTasks_server.Models.DTOs.Auth.Responses
{
    /// <summary>
    /// Resposta do início do processo de autenticação com o Google, contendo informações necessárias para redirecionar o usuário para a página de autorização do Google.
    /// </summary>
    public class GoogleAuthStartResponse
    {
        /// <summary>
        /// Código de sessão único gerado para a tentativa de autenticação, usado para rastrear o estado da autenticação.
        /// </summary>
        public Guid SessionCode { get; set; }

        /// <summary>
        /// URL de autorização do Google para redirecionar o usuário, onde ele pode conceder permissões à aplicação.
        /// </summary>
        public string AuthorizationUrl { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora de expiração do código de sessão, após a qual o código não será mais válido para autenticação.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
