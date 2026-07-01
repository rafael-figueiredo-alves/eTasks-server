namespace eTasks_server.Models.DTOs.Auth.Responses
{
    /// <summary>
    /// Resposta do resultado da recuperação de conta.
    /// </summary>
    public class AccountRecoveryResult
    {
        /// <summary>
        /// Indica se a recuperação de conta foi bem-sucedida.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Indica se o token de recuperação de conta expirou.
        /// </summary>
        public bool Expired { get; set; }

        /// <summary>
        /// Mensagem de resposta detalhando o resultado da recuperação de conta.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
