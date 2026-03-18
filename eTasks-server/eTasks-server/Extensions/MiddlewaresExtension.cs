using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Diagnostics;
using MySqlConnector;
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

                if (webApplication.Environment.IsDevelopment())
                {
                    webApplication.UseWebAssemblyDebugging();
                    webApplication.MapOpenApi();
                    webApplication.MapScalarApiReference();
                }
                else
                {
                    webApplication.UseExceptionHandler("/Error", createScopeForErrors: true);
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    webApplication.UseHsts();
                }

                webApplication.UseStatusCodePages(async statusCodeContext =>
                {
                    var response = statusCodeContext.HttpContext.Response;
                    if (response.StatusCode == 401 || response.StatusCode == 403 || response.StatusCode == 404)
                    {
                        response.ContentType = "application/json";
                        var message = response.StatusCode switch
                        {
                            401 => "Acesso não autorizado. Autenticação é necessária.",
                            403 => "Acesso negado. Você não tem permissão para acessar este recurso.",
                            404 => "Recurso não encontrado.",
                            _ => "Ocorreu um erro no processamento da requisição."
                        };
                        
                        var errorResponse = new eTasks_server.Models.Exceptions.ErrorResponse { StatusCode = response.StatusCode, Message = message };
                        await response.WriteAsJsonAsync(errorResponse);
                    }
                });
                
                webApplication.UseAuthentication();
                webApplication.UseAuthorization();
                
                webApplication.UseHttpsRedirection();
                webApplication.UseAntiforgery();
            }
        }
    }
}
