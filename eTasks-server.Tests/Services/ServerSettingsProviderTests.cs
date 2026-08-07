using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Entities.Settings;
using eTasks_server.Models.Utils;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace eTasks_server.Tests.Services
{
    /// <summary>
    /// Classe de testes para o ServerSettingsProvider, responsável por fornecer configurações do servidor.
    /// </summary>
    public class ServerSettingsProviderTests
    {
        /// <summary>
        /// Cria uma instância de ISecretProtector para uso nos testes.
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
        /// Testa se o método GetCurrentAsync descriptografa corretamente os campos sensíveis do ServerSettings.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetCurrentAsync_DecryptsSensitiveFields()
        {
            using var context = TestDbContextFactory.Create(nameof(GetCurrentAsync_DecryptsSensitiveFields));
            var protector = CreateProtector();

            context.ServerSettings.Add(new ServerSettings
            {
                SmtpPassword = protector.Protect("smtp-secret"),
                OpenRouterApiKey = protector.Protect("openrouter-secret"),
                MongoAuditConnectionString = protector.Protect("mongo-secret")
            });
            await context.SaveChangesAsync();

            var sut = new ServerSettingsProvider(context, protector);

            var settings = await sut.GetCurrentAsync();

            Assert.Equal("smtp-secret", settings.SmtpPassword);
            Assert.Equal("openrouter-secret", settings.OpenRouterApiKey);
            Assert.Equal("mongo-secret", settings.MongoAuditConnectionString);
        }
    }
}
