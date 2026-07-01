namespace eTasks_server.Models.DTOs.Auth.Responses
{
    /// <summary>
    /// Classe que representa a resposta do status da autenticação com o Google, fornecendo informações sobre o estado atual da tentativa de autenticação.
    /// </summary>
    public class GoogleAuthStatusResponse
    {
        /// <summary>
        /// Código de sessão único gerado para a tentativa de autenticação, usado para rastrear o estado da autenticação.
        /// </summary>
        public Guid SessionCode { get; set; }

        /// <summary>
        /// Status atual da autenticação, que pode indicar se a autenticação foi bem-sucedida, falhou ou está pendente.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Código de erro retornado pelo Google, caso a autenticação tenha falhado, fornecendo informações sobre o motivo da falha.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Descrição detalhada do erro retornado pelo Google, caso a autenticação tenha falhado, oferecendo mais contexto sobre o motivo da falha.
        /// </summary>
        public string? ErrorDescription { get; set; }

        /// <summary>
        /// Data e hora de expiração do código de sessão, após a qual o código não será mais válido para autenticação.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
