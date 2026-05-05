using eTasks_server.Middlewares;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Scalar.AspNetCore;

namespace eTasks_server.Extensions
{
    /// <summary>
    /// Extensão para registrar middlewares na aplicação, garantindo uma configuração centralizada e consistente para tratamento de erros, autenticação, autorização e outras funcionalidades transversais.
    /// </summary>
    public static class MiddlewaresExtension
    {
        extension(WebApplication webApplication)
        {
            /// <summary>
            /// Registrar Middlewares essenciais para a aplicação, incluindo CORS, tratamento de exceções, autenticação, autorização e configuração de endpoints de documentação da API. Esta configuração é aplicada globalmente para garantir que todas as requisições sejam processadas de forma consistente, com tratamento adequado de erros e segurança reforçada.
            /// </summary>
            public void RegisterMiddlewares()
            {
                //Adiciona o middleware de CORS para permitir requisições do frontend hospedado no GitHub Pages
                webApplication.UseCors(Constants.CorsPolicyName);

                // Middleware de tratamento de exceções para ambientes de desenvolvimento e produção
                webApplication.UseExceptionHandler();

                // Adiciona o middleware de documentação da API usando OpenAPI/Swagger, com um endpoint específico para a documentação de tipos escalares personalizados, protegida por autenticação.
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

                        var errorResponse = new ErrorResponse
                        {
                            TraceId = Guid.NewGuid().ToString(),
                            Message = message,
                            Details = string.Empty
                        };

                        await response.WriteAsJsonAsync(errorResponse);
                    }
                });

                //Adiciona o middleware de auditoria de operações para registrar ações dos usuários, como criação, atualização e exclusão de recursos, garantindo um histórico de atividades para fins de monitoramento e segurança.
                webApplication.UseMiddleware<OperationAuditMiddleware>();

                //Adiciona os middlewares de autenticação e autorização para proteger os endpoints da API, garantindo que apenas usuários autenticados e autorizados possam acessar recursos sensíveis.
                webApplication.UseAuthentication();
                webApplication.UseAuthorization();

                //Adiciona o middleware de redirecionamento para HTTPS e proteção contra CSRF, garantindo que todas as comunicações sejam seguras e que as requisições sejam protegidas contra ataques de falsificação de solicitação entre sites.
                webApplication.UseHttpsRedirection();
                webApplication.UseAntiforgery();
            }
        }
    }
}
