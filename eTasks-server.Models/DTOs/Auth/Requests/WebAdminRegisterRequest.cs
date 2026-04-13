namespace eTasks_server.Models.DTOs.Auth.Requests
{
    /// <summary>
    /// Dados do formulario de cadastro administrativo do painel web.
    /// </summary>
    public class WebAdminRegisterRequest
    {
        /// <summary>
        /// Identificador de login do administrador.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do administrador.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Nome de exibicao do administrador.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Chave fixa exigida para autorizar o cadastro administrativo.
        /// </summary>
        public string AdminKey { get; set; } = string.Empty;
    }
}
