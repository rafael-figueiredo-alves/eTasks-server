using eTasks_server.Core.BusinessLogicLayers.Auth;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Auth.Requests;
using eTasks_server.Models.Utils;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Auth
{
    /// <summary>
    /// Realiza testes unitários para a camada de lógica de negócios de autenticação (AuthBLL).
    /// </summary>
    public class AuthBLLTests
    {
        /// <summary>
        /// Cria uma configuração de teste com valores simulados para as chaves de criptografia, JWT e URL base da API.
        /// </summary>
        /// <returns></returns>
        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [Constants.DataEncryptionKeyConfig] = "unit-test-encryption-key",
                    [Constants.JwtKeyConfig] = "jwt-secret-key-with-at-least-thirty-two-chars",
                    [Constants.JwtIssuerConfig] = "issuer",
                    [Constants.JwtAudienceConfig] = "audience",
                    [Constants.ApiBaseUrl] = "http://localhost:5033"
                })
                .Build();
        }

        /// <summary>
        /// Cria uma instância de ISecretProtector usando a configuração fornecida.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static ISecretProtector CreateProtector(IConfiguration configuration) => new SecretProtector(configuration);

        /// <summary>
        /// Cria uma instância de IAuthBLL (AuthBLL) para testes, injetando dependências simuladas e reais.
        /// </summary>
        /// <param name="context">Contexto de banco de dados para testes</param>
        /// <param name="configuration">Configuração de teste</param>
        /// <param name="emailService">Serviço de email simulado</param>
        /// <param name="protector">Protector de segredos</param>
        /// <returns></returns>
        private static IAuthBLL CreateSut(Core.Data.AppDbContext context, IConfiguration configuration, IEmailService emailService, ISecretProtector protector)
        {
            var services = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            return new AuthBLL(
                context,
                configuration,
                emailService,
                protector,
                new ServerSettingsProvider(context, protector),
                new AccountDeletionRetentionService(context, NullLogger<IAccountDeletionRetentionService>.Instance),
                services.GetRequiredService<IHttpClientFactory>(),
                NullLogger<IAuthBLL>.Instance);
        }

        /// <summary>
        /// Testa se o método RegisterAsync armazena corretamente o hash de senha protegido e cria um token de atualização (refresh token) ao registrar um novo usuário.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterAsync_StoresProtectedPasswordHash_AndCreatesRefreshToken()
        {
            using var context = TestDbContextFactory.Create(nameof(RegisterAsync_StoresProtectedPasswordHash_AndCreatesRefreshToken));
            var configuration = CreateConfiguration();
            var protector = CreateProtector(configuration);
            var emailService = new FakeEmailService();
            IAuthBLL sut = CreateSut(context, configuration, emailService, protector);

            var response = await sut.RegisterAsync(new RegisterRequest
            {
                Name = "User Test",
                Email = "user@test.com",
                Password = "123456",
                UserAgent = Constants.WebUserAgent
            });

            var user = context.Users.Single();

            Assert.StartsWith("enc::", user.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("123456", protector.Unprotect(user.PasswordHash)));
            Assert.Single(context.RefreshTokens);
            Assert.False(string.IsNullOrWhiteSpace(response.Token));
            Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        }

        /// <summary>
        /// Testa se o método LoginAsync retorna um novo token de atualização (refresh token) ao fazer login com uma senha protegida corretamente.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LoginAsync_WithProtectedPasswordHash_ReturnsNewRefreshToken()
        {
            using var context = TestDbContextFactory.Create(nameof(LoginAsync_WithProtectedPasswordHash_ReturnsNewRefreshToken));
            var configuration = CreateConfiguration();
            var protector = CreateProtector(configuration);
            var emailService = new FakeEmailService();
            var user = TestDbContextFactory.CreateActiveUser();
            user.PasswordHash = protector.Protect(BCrypt.Net.BCrypt.HashPassword("123456"));
            context.Users.Add(user);
            await context.SaveChangesAsync();

            IAuthBLL sut = CreateSut(context, configuration, emailService, protector);

            var response = await sut.LoginAsync(new LoginRequest
            {
                Email = user.Email,
                Password = "123456",
                UserAgent = Constants.WebUserAgent
            }, "127.0.0.1");

            Assert.False(string.IsNullOrWhiteSpace(response.Token));
            Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
            Assert.Single(context.RefreshTokens);
            Assert.Contains(context.LoginLogs, x => x.UserUid == user.Uid && x.Status == "Success");
        }

        /// <summary>
        /// Testa se o método ChangePasswordAsync atualiza corretamente o hash de senha protegido e revoga os tokens de atualização (refresh tokens) ativos do usuário.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ChangePasswordAsync_UpdatesProtectedHash_AndRevokesActiveTokens()
        {
            using var context = TestDbContextFactory.Create(nameof(ChangePasswordAsync_UpdatesProtectedHash_AndRevokesActiveTokens));
            var configuration = CreateConfiguration();
            var protector = CreateProtector(configuration);
            var emailService = new FakeEmailService();
            var user = TestDbContextFactory.CreateActiveUser();
            user.PasswordHash = protector.Protect(BCrypt.Net.BCrypt.HashPassword("123456"));
            context.Users.Add(user);
            context.RefreshTokens.Add(new Models.Entities.Users.RefreshToken
            {
                UserUid = user.Uid,
                Token = "active-token",
                ExpiresAt = DateTime.UtcNow.AddDays(10),
                IsRevoked = false
            });
            await context.SaveChangesAsync();

            IAuthBLL sut = CreateSut(context, configuration, emailService, protector);

            var changed = await sut.ChangePasswordAsync(user.Uid, new ChangePasswordRequest
            {
                CurrentPassword = "123456",
                NewPassword = "654321"
            });

            var updatedUser = context.Users.Single();
            var token = context.RefreshTokens.Single();

            Assert.True(changed);
            Assert.True(token.IsRevoked);
            Assert.True(BCrypt.Net.BCrypt.Verify("654321", protector.Unprotect(updatedUser.PasswordHash)));
        }
    }
}
