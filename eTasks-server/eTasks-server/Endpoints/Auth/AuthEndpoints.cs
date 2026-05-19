using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Extensions;
using eTasks_server.Models.DTOs.Auth.Requests;
using eTasks_server.Models.DTOs.Auth.Responses;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace eTasks_server.Endpoints.Auth
{
    public static class AuthEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints relacionados à autenticação, incluindo login, registro, refresh de token, logout, esqueci minha senha, reset de senha, alteração de senha e confirmação de conta. Também inclui endpoints para o fluxo de login OAuth/OpenID Connect com Google. Os endpoints são organizados em um grupo com a rota base "/auth" e possuem tags, resumos, descrições e tipos de resposta adequados para cada operação.
            /// </summary>
            /// <returns></returns>
            public IEndpointRouteBuilder MapAuthEndpoints()
            {
                var group = app.MapGroup("/auth")
                    .WithTags("Autenticacao");

                group.LoginEndpoint()
                     .RegisterEndpoint()
                     .RefreshTokenEndpoint()
                     .LogoutEndpoint()
                     .ForgotPasswordEndpoint()
                     .ResetPasswordEndpoint()
                     .ChangePasswordEndpoint()
                     .ConfirmAccountEndpoint()
                     .RecoverDeletedAccountEndpoint()
                     .GoogleAuthEndpoints();

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            /// <summary>
            /// Método para realizar login no sistema e obter um JWT e Refresh Token. O JWT deve ser enviado no header Authorization Bearer para acessar rotas protegidas. O Refresh Token pode ser usado para obter um novo JWT sem precisar fazer login novamente, desde que o Refresh Token seja válido e não tenha expirado.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder LoginEndpoint()
            {
                group.MapPost("/login", async (HttpContext context, [FromBody] LoginRequest request, IAuthBLL authBLL) =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString();
                    var response = await authBLL.LoginAsync(request, ip);

                    SetRefreshTokenCookie(context, response, request.UserAgent);

                    return Results.Ok(response);
                })
                .WithName("Login")
                .AllowAnonymous()
                .WithDisplayName("Login de Usuário")
                .WithSummary("Realiza o login de um usuário retornando JWT e Refresh Token.")
                .WithDescription("O JWT deve ser enviado no header Authorization Bearer para acessar rotas protegidas. O Refresh Token pode ser usado para obter um novo JWT sem precisar fazer login novamente, desde que o Refresh Token seja válido e não tenha expirado.")
                .Produces(StatusCodes.Status200OK, typeof(LoginResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Endpoint para registrar um novo usuário no sistema. Após o registro, um e-mail de confirmação será enviado. O usuário deve clicar no link de confirmação para ativar a conta antes de poder fazer login.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder RegisterEndpoint()
            {
                group.MapPost("/register", async (HttpContext context, [FromBody] RegisterRequest request, IAuthBLL authBLL) =>
                {
                    var response = await authBLL.RegisterAsync(request);

                    SetRefreshTokenCookie(context, response, request.UserAgent);

                    return Results.Ok(response);
                })
                .WithName("UserRegister")
                .WithDisplayName("Registro de Usuário")
                .WithSummary("Registra um novo usuário no sistema.")
                .WithDescription("Após o registro, um e-mail de confirmação será enviado. O usuário deve clicar no link de confirmação para ativar a conta antes de poder fazer login.")
                .AllowAnonymous()
                .Produces(StatusCodes.Status200OK, typeof(LoginResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse));

                return group;
            }

            /// <summary>
            /// Endpoint para trocar um Refresh Token válido e não expirado por um novo Token JWT. Utilizado para obter um novo JWT sem precisar fazer login novamente, desde que o Refresh Token seja válido e não tenha expirado. O JWT retornado deve ser usado no header Authorization Bearer para acessar rotas protegidas.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder RefreshTokenEndpoint()
            {
                group.MapPost("/refresh", async (HttpContext context, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] RefreshTokenRequest? request, IAuthBLL authBLL) =>
                {
                    request ??= new RefreshTokenRequest();

                    if (ShouldUseRefreshTokenCookie(request.UserAgent)
                        && string.IsNullOrWhiteSpace(request.RefreshToken)
                        && context.Request.Cookies.TryGetValue(Constants.RefreshTokenCookieName, out var cookieRefreshToken))
                    {
                        request.RefreshToken = cookieRefreshToken;
                    }

                    var response = await authBLL.RefreshTokenAsync(request);
                    SetRefreshTokenCookie(context, response, request.UserAgent);

                    return Results.Ok(response);
                })
                .WithName("RefreshToken")
                .AllowAnonymous()
                .WithSummary("Troca um Refresh Token válido e não expirado por um novo Token JWT.")
                .WithDisplayName("Logar com Refresh Token")
                .WithDescription("Utilizado para obter um novo JWT sem precisar fazer login novamente, desde que o Refresh Token seja válido e não tenha expirado. O JWT retornado deve ser usado no header Authorization Bearer para acessar rotas protegidas.")
                .Produces(StatusCodes.Status200OK, typeof(LoginResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Use este endpoint para revogar o Refresh Token atual, efetivamente fazendo logout do usuário. Ele também remove os cookies HttpOnly de autenticação. Após usar este endpoint, o usuário precisará fazer login novamente para obter um novo JWT e Refresh Token.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder LogoutEndpoint()
            {
                group.MapPost("/logout", async (HttpContext context, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] RefreshTokenRequest? request, IAuthBLL authBLL) =>
                {
                    if (string.IsNullOrWhiteSpace(request?.UserAgent))
                    {
                        return Results.ValidationProblem(new Dictionary<string, string[]>
                        {
                            ["UserAgent"] = ["O UserAgent é obrigatório."]
                        });
                    }

                    var refreshToken = request?.RefreshToken;

                    if (ShouldUseRefreshTokenCookie(request?.UserAgent)
                        && string.IsNullOrWhiteSpace(refreshToken)
                        && context.Request.Cookies.TryGetValue(Constants.RefreshTokenCookieName, out var cookieRefreshToken))
                    {
                        refreshToken = cookieRefreshToken;
                    }

                    await authBLL.RevokeRefreshTokenAsync(refreshToken);

                    ClearRefreshTokenCookie(context);

                    return Results.Ok(new { Message = "Logout realizado com sucesso." });
                })
                .WithName("Logout")
                .AllowAnonymous()
                .WithSummary("Revoga o refresh token atual e remove os cookies HttpOnly de autenticação.")
                .WithDisplayName("Logout da API")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Endpoint para solicitar o envio de um código de redefinição de senha para o e-mail do usuário. Se o e-mail existir no sistema, um código de 6 dígitos será enviado para o endereço de e-mail fornecido. O usuário pode então usar esse código para redefinir a senha da conta usando o endpoint de reset de senha.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder ForgotPasswordEndpoint()
            {
                group.MapPost("/forgot-password", async ([FromBody] ForgotPasswordRequest request, IAuthBLL authBLL) =>
                {
                    var success = await authBLL.ForgotPasswordAsync(request);
                    return Results.Ok(new PasswordResponse { Success = success, Message = "Se o e-mail fornecido for válido, um código foi enviado para o endereço." });
                })
                .WithName("ForgotPassword")
                .AllowAnonymous()
                .WithDisplayName("Esqueci minha senha")
                .WithSummary("Solicita envio de código de redefinição de senha.")
                .WithDescription("Utilizado para solicitar o envio de um código de redefinição de senha para o e-mail do usuário. Se o e-mail existir no sistema, um código de 6 digitos será enviado.")
                .Produces(StatusCodes.Status200OK, typeof(PasswordResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Reseta a senha de um usuário usando um código de 6 dígitos enviado para o e-mail do usuário. O usuário deve fornecer o endereço de e-mail, o código de 6 dígitos recebido e a nova senha desejada. Se o código for válido e corresponder ao e-mail fornecido, a senha da conta será atualizada para a nova senha.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder ResetPasswordEndpoint()
            {
                group.MapPost("/reset-password", async ([FromBody] ResetPasswordRequest request, IAuthBLL authBLL) =>
                {
                    var success = await authBLL.ResetPasswordAsync(request);
                    return Results.Ok(new PasswordResponse { Success = success, Message = "Senha redefinida com êxito." });
                })
                .WithName("ResetPassword")
                .AllowAnonymous()
                .WithSummary("Valida o código de 6 digitos e altera a senha da conta.")
                .WithDescription("Utilizado para redefinir a senha de um usuário. O código enviado para o e-mail deve ser informado junto com a nova senha.")
                .WithDisplayName("Redefinir senha com código de 6 digitos")
                .Produces(StatusCodes.Status200OK, typeof(PasswordResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Endpoint para alterar a senha de um usuário autenticado via JWT Bearer. O usuário deve fornecer a senha atual e a nova senha. O JWT deve ser enviado no header Authorization Bearer. Se a senha atual for válida, a senha da conta será atualizada para a nova senha fornecida.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder ChangePasswordEndpoint()
            {
                group.MapPost("/change-password", async (System.Security.Claims.ClaimsPrincipal user, [FromBody] ChangePasswordRequest request, IAuthBLL authBLL) =>
                {
                    var userUid = user.GetRequiredUserUid();

                    var success = await authBLL.ChangePasswordAsync(userUid, request);
                    return Results.Ok(new PasswordResponse { Success = success, Message = "Senha alterada com êxito." });
                })
                .RequireAuthorization(policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                })
                .WithName("ChangePassword")
                .WithSummary("Altera a senha de um usuário autenticado via JWT Bearer.")
                .WithDisplayName("Alterar senha de usuário autenticado")
                .WithDescription("O usuario deve fornecer a senha atual e a nova senha. O JWT deve ser enviado no header Authorization Bearer.")
                .Produces(StatusCodes.Status200OK, typeof(PasswordResponse))
                .Produces(StatusCodes.Status400BadRequest, typeof(ErrorResponse))
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Endpoint para confirmar o endereço de e-mail de um usuário após o registro. O usuário deve clicar no link de confirmação enviado para o e-mail, que contém um token JWT como parâmetro de consulta. Este endpoint valida o token JWT e ativa a conta, garantindo a veracidade do endereço de e-mail fornecido durante o registro.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder ConfirmAccountEndpoint()
            {
                group.MapGet("/confirm-email", async ([FromQuery] string token, IAuthBLL authBLL) =>
                {
                    var success = await authBLL.ConfirmEmailAsync(token);
                    if (success)
                    {
                        return Results.Content("<h1>Conta confirmada com sucesso!</h1><p>Você pode fechar esta aba e retornar ao aplicativo.</p>", "text/html");
                    }

                    return Results.Content("<h1>O link expirou ou é inválido.</h1><p>Você pode solicitar um novo código direto pelo eTasks.</p>", "text/html");
                })
                .WithName("ConfirmEmail")
                .AllowAnonymous()
                .WithSummary("Valida o token JWT e ativa a conta garantindo a veracidade do endereço de e-mail.")
                .WithDescription("Utilizado para confirmar o endereço de e-mail de um usuário após o registro.")
                .WithDisplayName("Confirmar e-mail de registro")
                .Produces(StatusCodes.Status200OK, contentType: "text/html")
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            public RouteGroupBuilder RecoverDeletedAccountEndpoint()
            {
                group.MapGet("/recover-account", async (HttpContext context, [FromQuery] string code, IAuthBLL authBLL, CancellationToken cancellationToken) =>
                {
                    var result = await authBLL.RecoverDeletedAccountAsync(code, cancellationToken);
                    return Results.Content(BuildAccountRecoveryHtml(result, GetRequestBaseUri(context)), "text/html");
                })
                .WithName("RecoverDeletedAccount")
                .AllowAnonymous()
                .WithSummary("Reativa uma conta removida logicamente usando o link enviado por e-mail.")
                .WithDescription("Valida o codigo de reativacao enviado por e-mail apos a solicitacao de exclusao da conta. Se o codigo estiver valido, a conta volta a ficar ativa.")
                .WithDisplayName("Recuperar conta removida")
                .Produces(StatusCodes.Status200OK, contentType: "text/html")
                .Produces(StatusCodes.Status500InternalServerError);

                return group;
            }

            /// <summary>
            /// Métodos relacionados ao fluxo de login OAuth/OpenID Connect com Google. Este conjunto de endpoints permite que os usuários façam login usando suas contas do Google, utilizando o protocolo OAuth/OpenID Connect para autenticação. O fluxo inclui a obtenção da URL de autorização do Google, redirecionamento do usuário para o Google, processamento do callback do Google após a autenticação e consumo da sessão de login Google para obter um JWT e Refresh Token para acessar o sistema.
            /// </summary>
            /// <returns></returns>
            public RouteGroupBuilder GoogleAuthEndpoints()
            {
                group.GoogleOAuthLoginStartRetrieveAthURL()
                     .GoogleOAuthLoginStartRedirectURL()
                     .GoogleOAuthLoginCallback()
                     .GoogleOAuthLoginStatus()
                     .GoogleOAuthLoginConsume();

                return group;
            }

            #region Métodos relacionados ao fluxo de login OAuth/OpenID Connect com Google
            /// <summary>
            /// Retorna a URL de autorização do Google para iniciar o processo de login usando OAuth/OpenID Connect. Este endpoint é útil para cenários onde o cliente é um aplicativo móvel ou SPA que não pode lidar com redirecionamentos, permitindo que o cliente obtenha a URL de autorização e a utilize para iniciar o processo de login Google. Ele aceita os seguintes parâmetros no corpo da requisição: UserAgent (string, obrigatório), ClientInstanceId (string, obrigatório) e ReturnUrl (string, opcional). O endpoint retorna um objeto contendo a URL de autorização do Google, um código de sessão exclusivo e a data de expiração da sessão.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder GoogleOAuthLoginStartRetrieveAthURL()
            {
                group.MapPost("/google/start", async (HttpContext context, [FromBody] GoogleAuthStartRequest request, IAuthBLL authBLL, CancellationToken cancellationToken) =>
                {
                    var response = await authBLL.StartGoogleLoginAsync(request, GetRequestBaseUri(context), cancellationToken);
                    return Results.Ok(response);
                })
                .AllowAnonymous()
                .WithName("GoogleAuthStart")
                .WithSummary("Cria uma sessão de login Google OpenID Connect e retorna a URL de autorização.")
                .Produces(StatusCodes.Status200OK, typeof(GoogleAuthStartResponse));

                return group;
            }

            /// <summary>
            /// Redireciona o usuário para a URL de autorização do Google para iniciar o processo de login usando OAuth/OpenID Connect. Este endpoint é útil para cenários onde o cliente é um navegador web e pode lidar com redirecionamentos, permitindo uma experiência de login mais fluida. Ele aceita os mesmos parâmetros do endpoint de início de login, mas em vez de retornar a URL de autorização, ele redireciona diretamente o navegador do usuário para essa URL.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder GoogleOAuthLoginStartRedirectURL()
            {
                group.MapGet("/google/start", async (
                                                     HttpContext context,
                                                     [FromQuery] string userAgent,
                                                     [FromQuery] string clientInstanceId,
                                                     [FromQuery] string? returnUrl,
                                                     IAuthBLL authBLL,
                                                     CancellationToken cancellationToken) =>
                                                    {
                                                        var response = await authBLL.StartGoogleLoginAsync(new GoogleAuthStartRequest
                                                        {
                                                         UserAgent = userAgent,
                                                            ClientInstanceId = clientInstanceId,
                                                            ReturnUrl = returnUrl
                                                        }, GetRequestBaseUri(context), cancellationToken);

                                                        return Results.Redirect(response.AuthorizationUrl);
                                                    })
                .AllowAnonymous()
                .WithName("GoogleAuthStartRedirect")
                .WithSummary("Inicia o login Google por redirecionamento direto do navegador.");

                return group;
            }

            /// <summary>
            /// Método callback para processar a resposta do Google após o usuário concluir o processo de login no Google. O Google redirecionará o usuário de volta para este endpoint com os parâmetros de consulta code, state, error e error_description. Este endpoint processa esses parâmetros, completa o processo de login Google e, se bem-sucedido, pode redirecionar o usuário de volta para a aplicação cliente usando a URL de retorno fornecida durante o início do login ou exibir uma página de sucesso/erro informando o resultado do login Google.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder GoogleOAuthLoginCallback()
            {
                group.MapGet("/google/callback", async (
                    HttpContext context,
                    [FromQuery] string? code,
                    [FromQuery] string? state,
                    [FromQuery] string? error,
                    [FromQuery(Name = "error_description")] string? errorDescription,
                    IAuthBLL authBLL,
                    CancellationToken cancellationToken) =>
                {
                    var result = await authBLL.CompleteGoogleLoginAsync(code, state, error, errorDescription, context.Connection.RemoteIpAddress?.ToString(), GetRequestBaseUri(context), cancellationToken);
                    if (!string.IsNullOrWhiteSpace(result.RedirectUrl))
                    {
                        return Results.Redirect(result.RedirectUrl);
                    }

                    var title = result.Success ? "Login Google concluido" : "Login Google não concluído";
                    var html = $$"""
                    <!doctype html>
                    <html lang="pt-BR">
                    <head>
                        <meta charset="utf-8">
                        <meta name="viewport" content="width=device-width, initial-scale=1">
                        <title>{{title}}</title>
                    </head>
                    <body>
                        <h1>{{title}}</h1>
                        <p>{{WebUtility.HtmlEncode(result.Message)}}</p>
                        <p>Voce pode fechar esta janela e retornar ao eTasks.</p>
                        <script>window.close();</script>
                    </body>
                    </html>
                    """;

                    return Results.Content(html, "text/html");
                })
                .AllowAnonymous()
                .WithName("GoogleAuthCallback")
                .WithSummary("Callback OAuth/OpenID Connect usado pelo Google.");

                return group;
            }

            /// <summary>
            /// Obtém o status de uma sessão de login Google em andamento usando o código de sessão exclusivo gerado durante o início do processo de login. Este endpoint é projetado para ser chamado periodicamente pelo cliente (por exemplo, a cada poucos segundos) enquanto o usuário está no fluxo de login do Google para verificar se a autenticação foi concluída. Ele aceita os seguintes parâmetros de consulta: sessionCode (Guid, obrigatório), userAgent (string, obrigatório) e clientInstanceId (string, obrigatório). O endpoint retorna um objeto contendo o status atual da sessão de login Google, que pode ser "pending", "completed" ou "failed", juntamente com informações adicionais como data de expiração da sessão e detalhes de erros, se houver.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder GoogleOAuthLoginStatus()
            {
                group.MapGet("/google/status", async (
                                                      [FromQuery] Guid sessionCode,
                                                      [FromQuery] string userAgent,
                                                      [FromQuery] string clientInstanceId,
                                                      IAuthBLL authBLL,
                                                      CancellationToken cancellationToken) =>
                                                     {
                                                         var response = await authBLL.GetGoogleLoginStatusAsync(sessionCode, userAgent, clientInstanceId, cancellationToken);
                                                         return Results.Ok(response);
                                                     })
                 .AllowAnonymous()
                 .WithName("GoogleAuthStatus")
                 .WithSummary("Consulta o estado de uma sessão de login Google.")
                 .Produces(StatusCodes.Status200OK, typeof(GoogleAuthStatusResponse));

                return group;
            }

            /// <summary>
            /// Consome uma sessão de login Google concluída, trocando o código de sessão por um JWT e Refresh Token. Este endpoint é chamado pelo cliente após detectar que a autenticação Google foi concluída com sucesso (por exemplo, após o usuário ser redirecionado de volta do fluxo de login do Google). Ele aceita um código de sessão exclusivo gerado durante o início do processo de login Google e retorna um objeto contendo o JWT, Refresh Token e suas respectivas datas de expiração. O JWT retornado deve ser usado no header Authorization Bearer para acessar rotas protegidas.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder GoogleOAuthLoginConsume()
            {
                group.MapPost("/google/consume", async (HttpContext context, [FromBody] GoogleAuthConsumeRequest request, IAuthBLL authBLL, CancellationToken cancellationToken) =>
                {
                    var response = await authBLL.ConsumeGoogleLoginAsync(request, cancellationToken);
                    SetRefreshTokenCookie(context, response, request.UserAgent);
                    return Results.Ok(response);
                })
                .AllowAnonymous()
                .WithName("GoogleAuthConsume")
                .WithSummary("Consome uma sessão Google concluída e retorna LoginResponse JWT/refresh.")
                .Produces(StatusCodes.Status200OK, typeof(LoginResponse));

                return group;
            }
            #endregion

            #region Métodos de apoio para gerenciamento de cookies de Refresh Token
            private static void SetRefreshTokenCookie(HttpContext context, LoginResponse response, string? userAgent)
            {
                if (!ShouldUseRefreshTokenCookie(userAgent))
                {
                    ClearRefreshTokenCookie(context);
                    return;
                }

                context.Response.Cookies.Append(Constants.RefreshTokenCookieName, response.RefreshToken, BuildCookieOptions(response.RefreshTokenExpiresAt));
            }

            private static void ClearRefreshTokenCookie(HttpContext context)
            {
                var cookieOptions = BuildCookieOptions(null);
                context.Response.Cookies.Delete(Constants.RefreshTokenCookieName, cookieOptions);
            }

            private static CookieOptions BuildCookieOptions(DateTime? expiresAtUtc)
            {
                return new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/api",
                    Expires = expiresAtUtc.HasValue ? new DateTimeOffset(expiresAtUtc.Value) : null
                };
            }

            private static bool ShouldUseRefreshTokenCookie(string? userAgent)
            {
                return string.Equals(userAgent, Constants.WebUserAgent, StringComparison.OrdinalIgnoreCase);
            }

            private static Uri GetRequestBaseUri(HttpContext context)
            {
                var request = context.Request;
                return new Uri($"{request.Scheme}://{request.Host}/");
            }

            private static string BuildAccountRecoveryHtml(AccountRecoveryResult result, Uri requestBaseUri)
            {
                var title = result.Success
                    ? "Conta recuperada"
                    : result.Expired
                        ? "Prazo excedido"
                        : "Link invalido";

                var details = WebUtility.HtmlEncode(result.Message);
                var logoUrl = new Uri(requestBaseUri, "eTasks2.webp").ToString();
                var color = result.Success ? "#1b7f45" : "#9a3412";

                return $$"""
                <!doctype html>
                <html lang="pt-BR">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>{{title}} - eTasks</title>
                    <style>
                        body { margin: 0; font-family: Arial, sans-serif; background: #f6f7f9; color: #1f2937; }
                        main { min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 24px; box-sizing: border-box; }
                        section { width: min(520px, 100%); background: #fff; border: 1px solid #d9dde3; border-radius: 8px; padding: 32px; text-align: center; box-sizing: border-box; }
                        img { max-height: 72px; margin-bottom: 20px; }
                        h1 { margin: 0 0 12px; color: {{color}}; font-size: 28px; }
                        p { margin: 0; line-height: 1.5; font-size: 16px; }
                    </style>
                </head>
                <body>
                    <main>
                        <section>
                            <img src="{{logoUrl}}" alt="eTasks Logo">
                            <h1>{{title}}</h1>
                            <p>{{details}}</p>
                        </section>
                    </main>
                </body>
                </html>
                """;
            }
            #endregion
        }
    }
}
