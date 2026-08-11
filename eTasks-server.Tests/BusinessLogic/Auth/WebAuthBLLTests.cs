using eTasks_server.Core.BusinessLogicLayers.Auth;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Auth.Requests;
using eTasks_server.Models.Utils;
using eTasks_server.Tests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Auth
{
    /// <summary>
    /// Realiza testes unitários para a classe WebAuthBLL, que é responsável por gerenciar a autenticação de usuários na aplicação.
    /// </summary>
    public class WebAuthBLLTests
    {
        /// <summary>
        /// Cria uma instância de IConfiguration com configurações de teste em memória, incluindo a chave de criptografia de dados e a chave de API do administrador.
        /// </summary>
        /// <returns></returns>
        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [Constants.DataEncryptionKeyConfig] = "unit-test-encryption-key",
                    [Constants.AdminApiKeyConfig] = "admin-secret-key"
                })
                .Build();
        }

        /// <summary>
        /// Cria uma instância de ISecretProtector usando a configuração fornecida. O ISecretProtector é responsável por proteger e desproteger dados sensíveis, como senhas.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static ISecretProtector CreateProtector(IConfiguration configuration) => new SecretProtector(configuration);

        /// <summary>
        /// Teste para verificar se o método RegisterAdminAsync armazena corretamente o hash da senha protegido.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegisterAdminAsync_StoresProtectedPasswordHash()
        {
            using var context = TestDbContextFactory.Create(nameof(RegisterAdminAsync_StoresProtectedPasswordHash));
            var configuration = CreateConfiguration();
            var protector = CreateProtector(configuration);
            IWebAuthBLL sut = new WebAuthBLL(context, configuration, protector, NullLogger<IWebAuthBLL>.Instance);

            await sut.RegisterAdminAsync(new WebAdminRegisterRequest
            {
                Email = "admin@test.com",
                Password = "123456",
                DisplayName = "Admin User",
                AdminKey = "admin-secret-key"
            }, "127.0.0.1");

            var user = context.Users.Single();
            Assert.True(user.IsAdmin);
            Assert.StartsWith("enc::", user.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("123456", protector.Unprotect(user.PasswordHash)));
        }

        /// <summary>
        /// Teste para verificar se o método LoginAsync autentica corretamente um usuário com hash de senha protegido e registra um log de sucesso.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task LoginAsync_WithProtectedPasswordHash_SignsInAndWritesSuccessLog()
        {
            using var context = TestDbContextFactory.Create(nameof(LoginAsync_WithProtectedPasswordHash_SignsInAndWritesSuccessLog));
            var configuration = CreateConfiguration();
            var protector = CreateProtector(configuration);
            var authService = new FakeAuthenticationService();
            var services = new ServiceCollection()
                .AddSingleton<IAuthenticationService>(authService)
                .BuildServiceProvider();
            var httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };

            var user = TestDbContextFactory.CreateActiveUser();
            user.IsAdmin = true;
            user.PasswordHash = protector.Protect(BCrypt.Net.BCrypt.HashPassword("123456"));
            context.Users.Add(user);
            await context.SaveChangesAsync();

            IWebAuthBLL sut = new WebAuthBLL(context, configuration, protector, NullLogger<IWebAuthBLL>.Instance);

            await sut.LoginAsync(httpContext, new WebLoginRequest
            {
                Email = user.Email,
                Password = "123456",
                RememberMe = false
            }, "127.0.0.1");

            Assert.NotNull(authService.SignedInPrincipal);
            Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, authService.SignInScheme);
            Assert.Contains(context.LoginLogs, x => x.UserUid == user.Uid && x.Status == "Success");
        }
    }
}
