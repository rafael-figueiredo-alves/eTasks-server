using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace eTasks_server.Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para a classe SecretProtector.
    /// </summary>
    public class SecretProtectorTests
    {
        /// <summary>
        /// Cria uma instância de ISecretProtector para uso nos testes.
        /// </summary>
        /// <returns></returns>
        private static ISecretProtector CreateSut()
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
        /// Testa se o método Protect e Unprotect funcionam corretamente em conjunto, garantindo que o valor original seja recuperado após a proteção e desproteção.
        /// </summary>
        [Fact]
        public void Protect_AndUnprotect_RoundTripOriginalValue()
        {
            var sut = CreateSut();

            var protectedValue = sut.Protect("sensitive-value");
            var plainValue = sut.Unprotect(protectedValue);

            Assert.StartsWith("enc::", protectedValue);
            Assert.Equal("sensitive-value", plainValue);
        }

        /// <summary>
        /// Testa se o método Protect retorna o mesmo payload quando o valor já está protegido, garantindo que não haja alterações desnecessárias.
        /// </summary>
        [Fact]
        public void Protect_WhenValueAlreadyProtected_ReturnsSamePayload()
        {
            var sut = CreateSut();
            var protectedValue = sut.Protect("abc123");

            var protectedAgain = sut.Protect(protectedValue);

            Assert.Equal(protectedValue, protectedAgain);
        }

        /// <summary>
        /// Testa se o método Unprotect retorna o valor original quando o valor fornecido é texto simples, garantindo que não haja alterações desnecessárias.
        /// </summary>
        [Fact]
        public void Unprotect_WhenValueIsPlainText_ReturnsOriginalValue()
        {
            var sut = CreateSut();

            var result = sut.Unprotect("plain-text");

            Assert.Equal("plain-text", result);
        }
    }
}
