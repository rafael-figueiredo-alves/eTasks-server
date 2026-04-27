using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.DTOs.ServerSettings.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace eTasks_server.Tests.Support
{
    internal sealed class FakeEmailService : IEmailService
    {
        public List<(string ToEmail, string ResetCode)> PasswordResetEmails { get; } = [];
        public List<(string ToEmail, string ConfirmationLink)> ConfirmationEmails { get; } = [];

        public Task SendPasswordResetEmailAsync(string toEmail, string resetCode)
        {
            PasswordResetEmails.Add((toEmail, resetCode));
            return Task.CompletedTask;
        }

        public Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            ConfirmationEmails.Add((toEmail, confirmationLink));
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeServerSettingsDiagnosticsService : IServerSettingsDiagnosticsService
    {
        public UpdateServerSettingsRequest? LastEmailRequest { get; private set; }
        public UpdateServerSettingsRequest? LastOpenRouterRequest { get; private set; }
        public UpdateServerSettingsRequest? LastMongoRequest { get; private set; }

        public Task<ServerSettingsTestResultResponse> TestEmailAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            LastEmailRequest = request;
            return Task.FromResult(new ServerSettingsTestResultResponse { Success = true, Message = "ok" });
        }

        public Task<ServerSettingsTestResultResponse> TestOpenRouterAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            LastOpenRouterRequest = request;
            return Task.FromResult(new ServerSettingsTestResultResponse { Success = true, Message = "ok" });
        }

        public Task<ServerSettingsTestResultResponse> TestMongoAsync(UpdateServerSettingsRequest request, CancellationToken cancellationToken = default)
        {
            LastMongoRequest = request;
            return Task.FromResult(new ServerSettingsTestResultResponse { Success = true, Message = "ok" });
        }
    }

    internal sealed class FakeAuthenticationService : IAuthenticationService
    {
        public ClaimsPrincipal? SignedInPrincipal { get; private set; }
        public string? SignInScheme { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            SignInScheme = scheme;
            SignedInPrincipal = principal;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;
    }
}
