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
    public class WebAuthBLLTests
    {
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

        private static ISecretProtector CreateProtector(IConfiguration configuration) => new SecretProtector(configuration);

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
