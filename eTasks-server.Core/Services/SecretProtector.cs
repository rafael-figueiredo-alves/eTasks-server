using System.Security.Cryptography;
using System.Text;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.Extensions.Configuration;

namespace eTasks_server.Core.Services
{
    public class SecretProtector(IConfiguration configuration) : ISecretProtector
    {
        private const string Prefix = "enc::";
        private readonly byte[] _key = BuildKey(configuration);

        public string Protect(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return value;
            }

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(value);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            var payload = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

            return Prefix + Convert.ToBase64String(payload);
        }

        public string Unprotect(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return value;
            }

            var payload = Convert.FromBase64String(value[Prefix.Length..]);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var ivLength = aes.BlockSize / 8;
            var iv = new byte[ivLength];
            var cipherBytes = new byte[payload.Length - ivLength];
            Buffer.BlockCopy(payload, 0, iv, 0, ivLength);
            Buffer.BlockCopy(payload, ivLength, cipherBytes, 0, cipherBytes.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        private static byte[] BuildKey(IConfiguration configuration)
        {
            var keyMaterial = configuration[Constants.DataEncryptionKeyConfig]
                ?? configuration[Constants.JwtKeyConfig]
                ?? "default_very_secret_key_1234567890_min_32_chars!";

            return SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));
        }
    }
}
