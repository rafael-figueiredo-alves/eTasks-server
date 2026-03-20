using System.Net;

namespace eTasks_server.Models.Exceptions
{
    /// <summary>
    /// Exceção para erros de negócio / autenticação com código HTTP e mensagem amigável ao usuário.
    /// </summary>
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Mensagem amigável ao usuário final (usada como "message" na resposta JSON de erro).
        /// </summary>
        public string? UserMessage { get; set; }

        /// <summary>
        /// Conteúdo bruto da resposta HTTP (usado pelo cliente Blazor ao consumir a API).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Construtor para erros de negócio no servidor — recebe apenas a mensagem amigável.
        /// </summary>
        public ApiException(HttpStatusCode statusCode, string userMessage)
            : base(userMessage)
        {
            StatusCode = statusCode;
            UserMessage = userMessage;
        }

        /// <summary>
        /// Construtor para uso no cliente Blazor (BaseService) — mantém compatibilidade com o construtor de 3 parâmetros.
        /// </summary>
        public ApiException(HttpStatusCode statusCode, string? content, string message)
            : base(message)
        {
            StatusCode = statusCode;
            Content = content;
            UserMessage = message;
        }
    }
}
