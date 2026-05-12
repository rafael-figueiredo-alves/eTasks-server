using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Auth.Requests;
using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.Auth
{
    public static class WebAuthEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints relacionados à autenticação web, incluindo login, registro de administradores e logout. Esses endpoints permitem que os usuários façam login, registrem-se como administradores e efetuem logout, com redirecionamentos apropriados em caso de sucesso ou falha. Os endpoints são agrupados sob a rota "/web-auth" e possuem tags para facilitar a documentação e organização.
            /// </summary>
            /// <returns></returns>
            public IEndpointRouteBuilder MapWebAuthEndpoints()
            {
                var group = app.MapGroup("/web-auth").WithTags("Autenticação Web");

                group.Login()
                     .Register()
                     .Logout();

                return app;
            }
        }

        extension (RouteGroupBuilder group)
        {
            /// <summary>
            /// Efetua o login do usuário, validando as credenciais fornecidas e estabelecendo uma sessão autenticada. Em caso de sucesso, redireciona para a URL especificada no parâmetro returnUrl ou para a página inicial. Em caso de falha, redireciona de volta para a página de login com mensagens de erro apropriadas.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder Login()
            {
                group.MapPost("/login", async (HttpContext context, [FromForm] WebLoginRequest request, [FromQuery] string? returnUrl, IWebAuthBLL webAuthBLL) =>
                {
                    try
                    {
                        var ip = context.Connection.RemoteIpAddress?.ToString();

                        await webAuthBLL.LoginAsync(context, request, ip);

                        return Results.LocalRedirect(GetSafeReturnUrl(returnUrl));
                    }
                    catch (ApiException ex)
                    {
                        return Results.LocalRedirect(BuildLoginRedirect(returnUrl, ex.StatusCode == HttpStatusCode.Forbidden
                            ? "Acesso restrito. Apenas administradores podem acessar o painel."
                            : ex.Message));
                    }
                    catch
                    {
                        return Results.LocalRedirect(BuildLoginRedirect(returnUrl, "Não foi possível realizar o login."));
                    }
                })
                .AllowAnonymous()
                .DisableAntiforgery()
                .ExcludeFromDescription()
                .WithName("WebLogin");

                return group;
            }

            /// <summary>
            /// Registra um novo usuário administrador, criando uma conta com as credenciais fornecidas e redirecionando para a página de login em caso de sucesso ou para a página de registro com mensagens de erro apropriadas em caso de falha.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder Register()
            {
                group.MapPost("/register", async (HttpContext context, [FromForm] WebAdminRegisterRequest request, IWebAuthBLL webAuthBLL) =>
                {
                    try
                    {
                        var ip = context.Connection.RemoteIpAddress?.ToString();

                        await webAuthBLL.RegisterAdminAsync(request, ip);

                        return Results.LocalRedirect("/login?success=" + Uri.EscapeDataString("Administrador cadastrado com sucesso. Faca login para continuar."));
                    }
                    catch (ApiException ex)
                    {
                        return Results.LocalRedirect(BuildRegisterRedirect(ex.Message));
                    }
                    catch
                    {
                        return Results.LocalRedirect(BuildRegisterRedirect("Não foi possível realizar o cadastro administrativo."));
                    }
                })
                .AllowAnonymous()
                .ExcludeFromDescription()
                .WithName("WebAdminRegister");

                return group;
            }

            /// <summary>
            /// Efetua o logout do usuário, invalidando a sessão atual e redirecionando para a página de login ou para uma URL segura especificada no parâmetro returnUrl.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder Logout()
            {
                group.MapGet("/logout", async (HttpContext context, [FromQuery] string? returnUrl, IWebAuthBLL webAuthBLL) =>
                {
                    await webAuthBLL.LogoutAsync(context);

                    return Results.LocalRedirect(GetSafeReturnUrl(returnUrl, "/login"));
                })
                .AllowAnonymous()
                .ExcludeFromDescription()
                .WithName("WebLogout");

                return group;
            }
        }

        #region Métodos auxiliares para construção de URLs de redirecionamento
        private static string BuildLoginRedirect(string? returnUrl, string errorMessage)
        {
            var safeReturnUrl = GetSafeReturnUrl(returnUrl);
            return $"/login?returnUrl={Uri.EscapeDataString(safeReturnUrl)}&error={Uri.EscapeDataString(errorMessage)}";
        }

        private static string BuildRegisterRedirect(string errorMessage)
        {
            return $"/register?error={Uri.EscapeDataString(errorMessage)}";
        }

        private static string GetSafeReturnUrl(string? returnUrl, string fallback = "/")
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
            {
                return fallback;
            }

            return returnUrl;
        }
        #endregion
    }
}
