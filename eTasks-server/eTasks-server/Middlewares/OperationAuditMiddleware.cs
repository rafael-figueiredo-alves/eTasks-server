using System.Diagnostics;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Core.Services.Models;
using eTasks_server.Extensions;

namespace eTasks_server.Middlewares
{
    /// <summary>
    /// Middleware de captura de auditoria de operações para endpoints da API. Ele registra detalhes como método HTTP, caminho, status code, duração da requisição, informações do usuário e mensagens de erro.
    /// </summary>
    /// <param name="next">Delegate para o próximo middleware na pipeline</param>
    public class OperationAuditMiddleware(RequestDelegate next)
    {
        /// <summary>
        /// Método principal do middleware que é chamado para cada requisição HTTP. Ele verifica se a requisição é para um endpoint da API, inicia um cronômetro para medir a duração da requisição, e registra as informações de auditoria quando a resposta estiver prestes a ser enviada.
        /// </summary>
        /// <param name="context">Contexto HTTP da requisição</param>
        /// <param name="auditLogger">Serviço de logging de auditoria</param>
        /// <returns>Task representando a operação assíncrona</returns>
        public async Task InvokeAsync(HttpContext context, IOperationAuditLogger auditLogger)
        {
            //Se a requisoição não for para um endpoint da API, simplesmente passa para o próximo middleware sem registrar auditoria
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await next(context);
                return;
            }

            //Inicia o cronômetro para medir a duração da requisição
            var stopwatch = Stopwatch.StartNew();
            Exception? capturedException = null;

            context.Response.OnCompleted(async () =>
            {
                //Para contador de tempo assim que a resposta estiver prestes a ser enviada, garantindo que o tempo registrado seja o mais preciso possível
                stopwatch.Stop();

                Guid? userUid = null;

                if (context.User.Identity?.IsAuthenticated == true)
                {
                    try
                    {
                        userUid = context.User.GetRequiredUserUid();
                    }
                    catch
                    {
                    }
                }

                var operationLog = new OperationAuditLog
                {
                    TraceIdentifier = context.TraceIdentifier,
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value ?? string.Empty,
                    QueryString = string.IsNullOrWhiteSpace(context.Request.QueryString.Value) ? 
                                                                null : context.Request.QueryString.Value,
                    EndpointName = context.GetEndpoint()?.DisplayName,
                    ResourceName = ResolveResourceName(context.Request.Path.Value),
                    StatusCode = context.Response.StatusCode,
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    UserUid = userUid,
                    IsAuthenticated = context.User.Identity?.IsAuthenticated == true,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    ErrorMessage = capturedException?.Message
                };

                await auditLogger.LogAsync(operationLog);
            });

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                capturedException = ex;
                throw;
            }
        }

        /// <summary>
        /// Trata de capturar o nome do recurso a partir do caminho da requisição, assumindo uma estrutura como /api/{resource}/{id}
        /// </summary>
        /// <param name="path">Caminho da requisição HTTP</param>
        /// <returns>Nome do recurso ou null se não puder ser determinado</returns>
        private static string? ResolveResourceName(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length >= 3 ? segments[2] : null;
        }
    }
}
