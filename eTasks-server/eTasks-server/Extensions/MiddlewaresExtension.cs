using eTasks_server.Models.Utils;
using eTasks_server.Middlewares;
using Scalar.AspNetCore;

namespace eTasks_server.Extensions
{
    public static class MiddlewaresExtension
    {
        extension(WebApplication webApplication)
        {
            public void RegisterMiddlewares()
            {
                webApplication.UseCors(Constants.CorsPolicyName);

                webApplication.UseExceptionHandler();

                // Available in both Dev and Prod
                webApplication.MapOpenApi();
                webApplication.MapScalarApiReference($"/{Constants.ScalarDocEndpoint}", options => 
                {
                    options.WithOpenApiRoutePattern($"/openapi/{Constants.ApiVersion}.json");
                }).RequireAuthorization();

                if (!webApplication.Environment.IsDevelopment())
                {
                    webApplication.UseExceptionHandler("/Error", createScopeForErrors: true);
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    webApplication.UseHsts();
                }

                webApplication.UseStatusCodePages(async statusCodeContext =>
                {
                    var request = statusCodeContext.HttpContext.Request;
                    var response = statusCodeContext.HttpContext.Response;

                    // Só aplica para rotas da API
                    if (!request.Path.StartsWithSegments("/api"))
                        return;
                    
                    if (response.StatusCode == 401 || response.StatusCode == 403 || response.StatusCode == 404)
                    {
                        response.ContentType = "application/json";
                        var message = response.StatusCode switch
                        {
                            401 => "Acesso não autorizado. Faça login para continuar.",
                            403 => "Você não tem permissão para acessar este recurso.",
                            404 => "O recurso solicitado não foi encontrado.",
                            _ => "Ocorreu um erro no processamento da requisição."
                        };

                        var errorResponse = new eTasks_server.Models.Exceptions.ErrorResponse
                        {
                            TraceId = Guid.NewGuid().ToString(),
                            Message = message,
                            Details = string.Empty
                        };
                        await response.WriteAsJsonAsync(errorResponse);
                    }
                });
                
                webApplication.UseAuthentication();
                webApplication.UseAuthorization();
                webApplication.UseMiddleware<OperationAuditMiddleware>();
                
                webApplication.UseHttpsRedirection();
                webApplication.UseAntiforgery();
            }
        }
    }
}
