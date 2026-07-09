using System.Security.Cryptography;

namespace eTasks_server.Core.Helpers
{
    /// <summary>
    /// Classe com funções úteis compartilhadas
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Função para listar valores dos enums
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static string FormatWithOr(IEnumerable<string> items)
        {
            var list = items.ToList();

            if (list.Count == 0) return string.Empty;
            if (list.Count == 1) return list[0];

            return string.Join(", ", list.Take(list.Count - 1)) + " ou " + list[^1];
        }

        /// <summary>
        /// Função para escapar string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Escape(string value)
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        /// <summary>
        /// Gera código de reativação aleatório
        /// </summary>
        /// <returns></returns>
        public static string GenerateReactivationCode()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        }
    }
}
