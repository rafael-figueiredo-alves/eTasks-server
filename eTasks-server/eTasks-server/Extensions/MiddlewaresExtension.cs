using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Diagnostics;
using MySqlConnector;

namespace eTasks_server.Extensions
{
    public static class MiddlewaresExtension
    {
        extension(WebApplication webApplication)
        {
            public void RegisterMiddlewares()
            {
                webApplication.UseCors(Constants.CorsPolicyName);

                webApplication.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;
                        context.Response.ContentType = "application/json";
                        if (exception is MySqlException)
                        {
                            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                message = "Banco de dados indisponível. Tente novamente mais tarde."
                            });
                        }
                        else
                        {
                            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                message = "Erro interno inesperado."
                            });
                        }
                    });
                });

                if (webApplication.Environment.IsDevelopment())
                {
                    webApplication.UseWebAssemblyDebugging();
                }
                else
                {
                    webApplication.UseExceptionHandler("/Error", createScopeForErrors: true);
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    webApplication.UseHsts();
                }

                webApplication.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
                webApplication.UseHttpsRedirection();
                webApplication.UseAntiforgery();
            }
        }
    }
}
