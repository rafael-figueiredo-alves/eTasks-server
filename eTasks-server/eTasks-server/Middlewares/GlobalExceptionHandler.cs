using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace eTasks_server.Middlewares
{
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
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            var statusCode = StatusCodes.Status500InternalServerError;
            string message = "Erro interno inesperado.";

            if (exception is ValidationException validationExc)
            {
                var errors = validationExc.Errors
                    .SelectMany(kvp => kvp.Value.Select(err => new ErrorDetail { Campo = kvp.Key, Erro = err }))
                    .ToList();

                var validationResponse = new ErrorResponse
                {
                    StatusCode = 400,
                    Message = "Um ou mais campos falharam na validação",
                    Errors = errors
                };

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(validationResponse, cancellationToken);
                return true;
            }
            else if (exception is MySqlException)
            {
                statusCode = StatusCodes.Status503ServiceUnavailable;
                message = "Banco de dados indisponível. Tente novamente mais tarde.";
            }

            var genericResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message
            };

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(genericResponse, cancellationToken);

            return true;
        }
    }
}
