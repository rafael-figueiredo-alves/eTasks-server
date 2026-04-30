using System.Diagnostics;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Core.Services.Models;
using eTasks_server.Extensions;

namespace eTasks_server.Middlewares
{
    public class OperationAuditMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, IOperationAuditLogger auditLogger)
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            Exception? capturedException = null;

            context.Response.OnCompleted(async () =>
            {
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
                    QueryString = string.IsNullOrWhiteSpace(context.Request.QueryString.Value) ? null : context.Request.QueryString.Value,
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
