using eTasks_server.Core.Services;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace eTasks_server.Tests.Services
{
    public class SecretProtectorTests
    {
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

        [Fact]
        public void Protect_AndUnprotect_RoundTripOriginalValue()
        {
            var sut = CreateSut();

            var protectedValue = sut.Protect("sensitive-value");
            var plainValue = sut.Unprotect(protectedValue);

            Assert.StartsWith("enc::", protectedValue);
            Assert.Equal("sensitive-value", plainValue);
        }

        [Fact]
        public void Protect_WhenValueAlreadyProtected_ReturnsSamePayload()
        {
            var sut = CreateSut();
            var protectedValue = sut.Protect("abc123");

            var protectedAgain = sut.Protect(protectedValue);

            Assert.Equal(protectedValue, protectedAgain);
        }

        [Fact]
        public void Unprotect_WhenValueIsPlainText_ReturnsOriginalValue()
        {
            var sut = CreateSut();

            var result = sut.Unprotect("plain-text");

            Assert.Equal("plain-text", result);
        }
    }
}
