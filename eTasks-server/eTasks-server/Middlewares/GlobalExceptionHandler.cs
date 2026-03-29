using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using MySqlConnector;

namespace eTasks_server.Middlewares
{
    /// <summary>
    /// Middleware global para captura e tratamento de exceções não tratadas em toda a aplicação.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

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
