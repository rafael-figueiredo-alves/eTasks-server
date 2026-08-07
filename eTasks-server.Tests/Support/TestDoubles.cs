using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace eTasks_server.Tests.Support
{
    /// <summary>
    /// Simulação de um serviço de email para fins de teste. Em vez de enviar emails reais, este serviço armazena os detalhes dos emails enviados em listas internas, permitindo que os testes verifiquem se os emails foram "enviados" corretamente.
    /// </summary>
    internal sealed class FakeEmailService : IEmailService
    {
        /// <summary>
        /// Lista de emails de redefinição de senha enviados. Cada item da lista contém o email do destinatário e o código de redefinição associado.
        /// </summary>
        public List<(string ToEmail, string ResetCode)> PasswordResetEmails { get; } = [];

        /// <summary>
        /// Lista de emails de confirmação de conta enviados. Cada item da lista contém o email do destinatário e o link de confirmação associado.
        /// </summary>
        public List<(string ToEmail, string ConfirmationLink)> ConfirmationEmails { get; } = [];

        /// <summary>
        /// Lista de emails de reativação de conta enviados. Cada item da lista contém o email do destinatário, o link de reativação associado e a data de expiração do link.
        /// </summary>
        public List<(string ToEmail, string ReactivationLink, DateTime ExpiresAt)> ReactivationEmails { get; } = [];

        /// <summary>
        /// Simula o envio de um email de redefinição de senha. Em vez de enviar um email real, este método adiciona os detalhes do email (destinatário e código de redefinição) à lista PasswordResetEmails.
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="resetCode"></param>
        /// <returns></returns>
        public Task SendPasswordResetEmailAsync(string toEmail, string resetCode)
        {
            PasswordResetEmails.Add((toEmail, resetCode));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Simula o envio de um email de confirmação de conta. Em vez de enviar um email real, este método adiciona os detalhes do email (destinatário e link de confirmação) à lista ConfirmationEmails.
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="confirmationLink"></param>
        /// <returns></returns>
        public Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            ConfirmationEmails.Add((toEmail, confirmationLink));
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Simula o envio de um email de reativação de conta. Em vez de enviar um email real, este método adiciona os detalhes do email (destinatário, link de reativação e data de expiração) à lista ReactivationEmails.
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="reactivationLink"></param>
        /// <param name="expiresAt"></param>
        /// <returns></returns>
        public Task SendAccountReactivationEmailAsync(string toEmail, string reactivationLink, DateTime expiresAt)
        {
            ReactivationEmails.Add((toEmail, reactivationLink, expiresAt));
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Simulação de um serviço de diagnóstico de configurações do servidor para fins de teste. Em vez de realizar testes reais nas configurações do servidor, este serviço armazena os detalhes das solicitações de teste em propriedades internas, permitindo que os testes verifiquem se as solicitações foram feitas corretamente.
    /// </summary>
    internal sealed class FakeServerSettingsDiagnosticsService : IServerSettingsDiagnosticsService
    {
        /// <summary>
        /// Armazena a última solicitação de teste de email recebida. Esta propriedade é útil para verificar se o método TestEmailAsync foi chamado com os parâmetros corretos durante os testes.
        /// </summary>
        public UpdateServerSettingsRequest? LastEmailRequest { get; private set; }

        /// <summary>
        /// Armazena a última solicitação de teste de OpenRouter recebida. Esta propriedade é útil para verificar se o método TestOpenRouterAsync foi chamado com os parâmetros corretos durante os testes.
        /// </summary>
        public UpdateServerSettingsRequest? LastOpenRouterRequest { get; private set; }

        /// <summary>
        /// Armazena a última solicitação de teste de MongoDB recebida. Esta propriedade é útil para verificar se o método TestMongoAsync foi chamado com os parâmetros corretos durante os testes.
        /// </summary>
        public UpdateServerSettingsRequest? LastMongoRequest { get; private set; }

        /// <summary>
        /// Simula o teste das configurações de email do servidor. Em vez de realizar um teste real, este método armazena a solicitação recebida na propriedade LastEmailRequest e retorna uma resposta simulada indicando sucesso.
        /// </summary>
        /// <param name="request">A solicitação de teste de email.</param>
        /// <param name="cancellationToken">O token de cancelamento.</param>
        /// <returns></returns>
        public Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            LastEmailRequest = request;
            return Task.FromResult(new ServerSettingsTestResultResponse { Success = true, Message = "ok" });
        }

        /// <summary>
        /// Simula o teste das configurações do OpenRouter do servidor. Em vez de realizar um teste real, este método armazena a solicitação recebida na propriedade LastOpenRouterRequest e retorna uma resposta simulada indicando sucesso.
        /// </summary>
        /// <param name="request">A solicitação de teste do OpenRouter.</param>
        /// <param name="cancellationToken">O token de cancelamento.</param>
        /// <returns></returns>
        public Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            LastOpenRouterRequest = request;
            return Task.FromResult(new ServerSettingsTestResultResponse { Success = true, Message = "ok" });
        }

        /// <summary>
        /// Simula o teste das configurações do MongoDB do servidor. Em vez de realizar um teste real, este método armazena a solicitação recebida na propriedade LastMongoRequest e retorna uma resposta simulada indicando sucesso.
        /// </summary>
        /// <param name="request">A solicitação de teste do MongoDB.</param>
        /// <param name="cancellationToken">O token de cancelamento.</param>
        /// <returns></returns>
        public Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            LastMongoRequest = request;
            return Task.FromResult(new ServerSettingsTestResultResponse { Success = true, Message = "ok" });
        }
    }

    /// <summary>
    /// Simulação de um serviço de autenticação para fins de teste. Em vez de realizar autenticações reais, este serviço armazena os detalhes das tentativas de autenticação em propriedades internas, permitindo que os testes verifiquem se as operações de autenticação foram chamadas corretamente.
    /// </summary>
    internal sealed class FakeAuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Armazena o último principal (usuário) que foi "autenticado" com sucesso. Esta propriedade é útil para verificar se o método SignInAsync foi chamado com os parâmetros corretos durante os testes.
        /// </summary>
        public ClaimsPrincipal? SignedInPrincipal { get; private set; }

        /// <summary>
        /// Armazena o último esquema de autenticação usado para "autenticar" um usuário. Esta propriedade é útil para verificar se o método SignInAsync foi chamado com os parâmetros corretos durante os testes.
        /// </summary>
        public string? SignInScheme { get; private set; }

        /// <summary>
        /// Simula a autenticação de um usuário. Em vez de realizar uma autenticação real, este método retorna um resultado indicando que nenhuma autenticação foi realizada.
        /// </summary>
        /// <param name="context">O contexto HTTP.</param>
        /// <param name="scheme">O esquema de autenticação.</param>
        /// <returns></returns>
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        /// <summary>
        /// Simula o desafio de autenticação. Em vez de realizar um desafio real, este método não faz nada e retorna uma tarefa concluída.
        /// </summary>
        /// <param name="context">O contexto HTTP.</param>
        /// <param name="scheme">O esquema de autenticação.</param>
        /// <param name="properties">As propriedades de autenticação.</param>
        /// <returns></returns>
        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        /// <summary>
        /// Simula a negação de acesso (forbid) para um usuário. Em vez de realizar uma negação real, este método não faz nada e retorna uma tarefa concluída.
        /// </summary>
        /// <param name="context">O contexto HTTP.</param>
        /// <param name="scheme">O esquema de autenticação.</param>
        /// <param name="properties">As propriedades de autenticação.</param>
        /// <returns></returns>
        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        /// <summary>
        /// Simula o processo de autenticação de um usuário. Em vez de realizar uma autenticação real, este método armazena os detalhes da autenticação em propriedades internas, permitindo que os testes verifiquem se o método foi chamado corretamente.
        /// </summary>
        /// <param name="context">O contexto HTTP.</param>
        /// <param name="scheme">O esquema de autenticação.</param>
        /// <param name="principal">O principal (usuário) a ser autenticado.</param>
        /// <param name="properties">As propriedades de autenticação.</param>
        /// <returns></returns>
        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            SignInScheme = scheme;
            SignedInPrincipal = principal;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Simula o processo de logout de um usuário. Em vez de realizar um logout real, este método não faz nada e retorna uma tarefa concluída.
        /// </summary>
        /// <param name="context">O contexto HTTP.</param>
        /// <param name="scheme">O esquema de autenticação.</param>
        /// <param name="properties">As propriedades de autenticação.</param>
        /// <returns></returns>
        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;
    }
}
