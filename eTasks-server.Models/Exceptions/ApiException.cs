using System.Net;

/// <summary>
/// Este namespace contém a classe ApiException, que é usada para representar erros de negócio e autenticação na API, fornecendo um código HTTP e uma mensagem amigável ao usuário. Essa classe é projetada para ser lançada pelo servidor quando ocorre um erro de negócio ou autenticação, e também pode ser usada pelo cliente Blazor (BaseService) para capturar erros ao consumir a API, mantendo compatibilidade com o construtor de 3 parâmetros.
/// </summary>
namespace eTasks_server.Models.Exceptions
{
    /// <summary>
    /// Exceção para erros de negócio / autenticação com código HTTP e mensagem amigável ao usuário.
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Código HTTP associado ao erro (usado pelo servidor para definir o status da resposta e pelo cliente Blazor para identificar o tipo de erro).
        /// </summary>
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
