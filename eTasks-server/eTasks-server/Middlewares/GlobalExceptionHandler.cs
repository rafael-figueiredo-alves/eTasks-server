using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;
using MySqlConnector;

namespace eTasks_server.Middlewares
{
    /// <summary>
    /// Middleware global para captura e tratamento de exceções não tratadas em toda a aplicação.
    /// </summary>
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        /// <summary>
        /// Tenta tratar a exceção capturada, mapeando tipos específicos para respostas HTTP adequadas e mensagens amigáveis.
        /// </summary>
        /// <param name="httpContext">Contexto HTTP</param>
        /// <param name="exception">Exceção capturada</param>
        /// <param name="cancellationToken">Token para cancelar ação</param>
        /// <returns>Indica se a exceção foi tratada</returns>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = Guid.NewGuid().ToString();

            _logger.LogError(exception, "[{TraceId}] Exceção capturada: {Message}", traceId, exception.Message);

            var statusCode = StatusCodes.Status500InternalServerError;
            var message = "Ocorreu um erro inesperado. Tente novamente ou entre em contato com o suporte.";
            var details = string.Empty;

            if (exception is ValidationException validationExc)
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = "Verifique os campos e tente novamente.";

                // Concatena os erros de validação em "details" de forma legível
                details = string.Join("; ", validationExc.Errors
                    .SelectMany(kvp => kvp.Value.Select(err => $"{kvp.Key}: {err}")));
            }
            else if (exception is ApiException apiExc)
            {
                statusCode = (int)apiExc.StatusCode;
                message = apiExc.UserMessage ?? apiExc.Message;
            }
            else if (exception is AntiforgeryValidationException)
            {
                // Token antiforgery inválido: redireciona ao login com mensagem amigável
                // em vez de exibir JSON bruto no browser
                var path = httpContext.Request.Path.Value ?? string.Empty;
                if (path.Contains("/web-auth/", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = httpContext.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
                    var safeReturn = Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ? returnUrl : "/";
                    var location = $"/login?returnUrl={Uri.EscapeDataString(safeReturn)}&error={Uri.EscapeDataString("Sessão expirada. Tente novamente.")}"; 
                    httpContext.Response.Redirect(location);
                    return true;
                }
                statusCode = StatusCodes.Status400BadRequest;
                message = "Requisição inválida. Recarregue a página e tente novamente.";
            }
            else if (exception is MySqlException)
            {
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = "Serviço temporariamente indisponível. Tente novamente em instantes.";
            }

            var errorResponse = new ErrorResponse
            {
                TraceId = traceId,
                Message = message,
                Details = details
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}
