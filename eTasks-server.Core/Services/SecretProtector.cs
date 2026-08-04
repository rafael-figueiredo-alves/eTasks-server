using System.Security.Cryptography;
using System.Text;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.Utils;
using Microsoft.Extensions.Configuration;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Serviço protetor de segredos que utiliza criptografia simétrica para proteger e desproteger valores sensíveis.
    /// </summary>
    /// <param name="configuration"></param>
    public class SecretProtector(IConfiguration configuration) : ISecretProtector
    {
        private const string Prefix = "enc::";
        private readonly byte[] _key = BuildKey(configuration);

        /// <summary>
        /// Protege um valor sensível, criptografando-o e adicionando um prefixo para indicar que está protegido.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Protect(string value)
        {
            // Valida se o valor é nulo ou vazio, retornando-o sem alterações.
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            // Verifica se o valor já está protegido, retornando-o sem alterações.
            if (value.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return value;
            }

            // Cria uma instância do algoritmo AES para criptografia simétrica.
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Cria um criptografador e realiza a criptografia do valor.
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(value);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            var payload = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

            // Retorna o valor protegido, codificado em Base64 e com o prefixo adicionado.
            return Prefix + Convert.ToBase64String(payload);
        }
        
        /// <summary>
        /// Desprotege um valor protegido, descriptografando-o e removendo o prefixo.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Unprotect(string value)
        {
            // Valida se o valor é nulo, vazio ou não possui o prefixo, retornando-o sem alterações.
            if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return value;
            }

            // Decodifica o valor protegido de Base64 e separa o vetor de inicialização (IV) dos bytes cifrados.
            var payload = Convert.FromBase64String(value[Prefix.Length..]);

            // Cria uma instância do algoritmo AES para descriptografia simétrica.
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

            // Cria um descriptografador e realiza a descriptografia dos bytes cifrados.
            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            // Retorna o valor desprotegido, convertido de bytes para string.
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Método privado para construir a chave de criptografia a partir da configuração fornecida. Se não houver uma chave configurada, será utilizada uma chave padrão.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static byte[] BuildKey(IConfiguration configuration)
        {
            var keyMaterial = configuration[Constants.DataEncryptionKeyConfig]
                ?? configuration[Constants.JwtKeyConfig]
                ?? "default_very_secret_key_1234567890_min_32_chars!";

            return SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));
        }
    }
}
