using eTasks_server.Core.BusinessLogicLayers.Admin.ServerSettings;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ServerSettings.Requests;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Admin
{
    /// <summary>
    /// Realiza testes unitários para a classe ServerSettingsAdminBLL, que é responsável por gerenciar operações administrativas relacionadas às configurações do servidor.
    /// </summary>
    public class ServerSettingsAdminBLLTests
    {
        /// <summary>
        /// Cria uma instância de ISecretProtector para uso nos testes, utilizando uma chave de criptografia de teste.
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
        /// Testa o método UpdateAsync da classe ServerSettingsAdminBLL, verificando se os campos sensíveis são criptografados corretamente e se o payload retornado contém os valores descriptografados.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAsync_EncryptsSensitiveFields_AndReturnsDecryptedPayload()
        {
            using var context = TestDbContextFactory.Create(nameof(UpdateAsync_EncryptsSensitiveFields_AndReturnsDecryptedPayload));
            var protector = CreateProtector();
            var diagnostics = new FakeServerSettingsDiagnosticsService();
            IServerSettingsAdminBLL sut = new ServerSettingsAdminBLL(context, protector, diagnostics, NullLogger<IServerSettingsAdminBLL>.Instance);

            var result = await sut.UpdateAsync(new UpdateServerSettingsRequest
            {
                SmtpEnabled = true,
                SmtpHost = "smtp.example.com",
                SmtpFromEmail = "noreply@example.com",
                SmtpPassword = "smtp-secret",
                OpenRouterEnabled = true,
                OpenRouterApiKey = "openrouter-secret",
                OpenRouterModel = "meta-llama/llama-3.3-8b-instruct:free",
                MongoAuditEnabled = true,
                MongoAuditConnectionString = "mongodb://localhost:27017"
            });

            var entity = context.ServerSettings.Single();

            Assert.Equal("smtp-secret", result.SmtpPassword);
            Assert.Equal("openrouter-secret", result.OpenRouterApiKey);
            Assert.Equal("mongodb://localhost:27017", result.MongoAuditConnectionString);
            Assert.StartsWith("enc::", entity.SmtpPassword);
            Assert.StartsWith("enc::", entity.OpenRouterApiKey);
            Assert.StartsWith("enc::", entity.MongoAuditConnectionString);
        }

        /// <summary>
        /// Testa o método UpdateAsync da classe ServerSettingsAdminBLL, verificando se uma exceção de validação é lançada quando a opção OpenRouterEnabled está ativada, mas a chave de API do OpenRouter não é fornecida.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAsync_WhenOpenRouterEnabledWithoutKey_ThrowsValidationException()
        {
            using var context = TestDbContextFactory.Create(nameof(UpdateAsync_WhenOpenRouterEnabledWithoutKey_ThrowsValidationException));
            var protector = CreateProtector();
            var diagnostics = new FakeServerSettingsDiagnosticsService();
            IServerSettingsAdminBLL sut = new ServerSettingsAdminBLL(context, protector, diagnostics, NullLogger<IServerSettingsAdminBLL>.Instance);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => sut.UpdateAsync(new UpdateServerSettingsRequest
            {
                OpenRouterEnabled = true,
                OpenRouterApiKey = "",
                OpenRouterModel = "meta-llama/llama-3.3-8b-instruct:free"
            }));

            Assert.Contains(nameof(UpdateServerSettingsRequest.OpenRouterApiKey), ex.Errors.Keys);
        }
    }
}
