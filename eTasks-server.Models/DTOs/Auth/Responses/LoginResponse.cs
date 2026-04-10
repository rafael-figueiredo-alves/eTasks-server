namespace eTasks_server.Models.DTOs.Auth.Responses
{
    /// <summary>
    /// Entidade de resposta para o login, contendo o token de acesso, token de atualização e suas respectivas datas de expiração.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Token de acesso gerado para o usuário autenticado, utilizado para autorizar requisições subsequentes.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Token de atualização gerado para o usuário autenticado, utilizado para obter um novo token de acesso quando o atual expirar.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Qaundo o token de acesso expira, indicando a data e hora em que o token se tornará inválido.
        /// </summary>
        public DateTime? TokenExpiresAt { get; set; }

        /// <summary>
        /// Quando o token de atualização expira, indicando a data e hora em que o token de atualização se tornará inválido.
        /// </summary>
        public DateTime? RefreshTokenExpiresAt { get; set; }
    }
}
