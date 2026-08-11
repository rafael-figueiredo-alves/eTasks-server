using eTasks_server.Core.BusinessLogicLayers.Admin;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Utils;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Admin
{
    /// <summary>
    /// Realiza testes unitários para a classe UserAdminBLL, que é responsável por gerenciar operações administrativas relacionadas a usuários, como redefinição de senha, bloqueio de usuários e envio de e-mails de redefinição de senha.
    /// </summary>
    public class UserAdminBLLTests
    {
        /// <summary>
        /// Cria uma instância de ISecretProtector para uso nos testes, utilizando uma chave de criptografia de teste definida na configuração em memória.
        /// </summary>
        /// <returns></returns>
        private static ISecretProtector CreateProtector()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [Constants.DataEncryptionKeyConfig] = "unit-test-encryption-key"
                })
                .Build();

            return new SecretProtector(configuration);
        }

        /// <summary>
        /// Testa o método SetPasswordAsync da classe UserAdminBLL, verificando se a senha do usuário é criptografada corretamente e se os tokens de atualização ativos são revogados após a alteração da senha.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SetPasswordAsync_EncryptsHash_AndRevokesActiveTokens()
        {
            using var context = TestDbContextFactory.Create(nameof(SetPasswordAsync_EncryptsHash_AndRevokesActiveTokens));
            var protector = CreateProtector();
            var emailService = new FakeEmailService();
            var user = TestDbContextFactory.CreateActiveUser();
            user.PasswordHash = protector.Protect(BCrypt.Net.BCrypt.HashPassword("old-password"));

            context.Users.Add(user);
            context.RefreshTokens.Add(new RefreshToken
            {
                UserUid = user.Uid,
                Token = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await context.SaveChangesAsync();

            IUserAdminBLL sut = new UserAdminBLL(context, emailService, protector, NullLogger<IUserAdminBLL>.Instance);

            var result = await sut.SetPasswordAsync(user.Uid, "new-password");

            var updatedUser = context.Users.Single();
            Assert.True(result);
            Assert.StartsWith("enc::", updatedUser.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("new-password", protector.Unprotect(updatedUser.PasswordHash)));
            Assert.All(context.RefreshTokens, x => Assert.True(x.IsRevoked));
        }

        /// <summary>
        /// Testa o método ToggleBlockAsync da classe UserAdminBLL, verificando se o usuário é bloqueado corretamente e se os tokens de atualização ativos são revogados após o bloqueio do usuário.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ToggleBlockAsync_BlocksUser_AndRevokesActiveTokens()
        {
            using var context = TestDbContextFactory.Create(nameof(ToggleBlockAsync_BlocksUser_AndRevokesActiveTokens));
            var protector = CreateProtector();
            var emailService = new FakeEmailService();
            var user = TestDbContextFactory.CreateActiveUser();
            context.Users.Add(user);
            context.RefreshTokens.Add(new RefreshToken
            {
                UserUid = user.Uid,
                Token = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await context.SaveChangesAsync();

            IUserAdminBLL sut = new UserAdminBLL(context, emailService, protector, NullLogger<IUserAdminBLL>.Instance);

            var result = await sut.ToggleBlockAsync(user.Uid);

            Assert.True(result);
            Assert.True(context.Users.Single().IsBlocked);
            Assert.All(context.RefreshTokens, x => Assert.True(x.IsRevoked));
        }

        /// <summary>
        /// Testa o método SendPasswordResetEmailAsync da classe UserAdminBLL, verificando se um código de redefinição de senha é criado corretamente e se o e-mail de redefinição de senha é enviado para o usuário.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SendPasswordResetEmailAsync_CreatesResetCode_AndDispatchesEmail()
        {
            using var context = TestDbContextFactory.Create(nameof(SendPasswordResetEmailAsync_CreatesResetCode_AndDispatchesEmail));
            var protector = CreateProtector();
            var emailService = new FakeEmailService();
            var user = TestDbContextFactory.CreateActiveUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            IUserAdminBLL sut = new UserAdminBLL(context, emailService, protector, NullLogger<IUserAdminBLL>.Instance);

            var result = await sut.SendPasswordResetEmailAsync(user.Uid);

            Assert.True(result);
            Assert.Single(context.PasswordResetCodes);
            Assert.Single(emailService.PasswordResetEmails);
            Assert.Equal(user.Email, emailService.PasswordResetEmails[0].ToEmail);
        }
    }
}
