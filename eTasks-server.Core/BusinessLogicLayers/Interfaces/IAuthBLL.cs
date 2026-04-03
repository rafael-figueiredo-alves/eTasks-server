using eTasks_server.Models.Auth;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface para a camada de negócios responsável por autenticação e gerenciamento de usuários.
    /// </summary>
    public interface IAuthBLL
    {
        /// <summary>
        /// Método para autenticar um usuário e gerar tokens de acesso e refresh. O endereço IP é opcional, mas pode ser usado para fins de segurança e auditoria.
        /// </summary>
        /// <param name="request">Recebe uma entidade de LoginRequest</param>
        /// <param name="ipAddress">Recebe o endereço de IP do usuário que fez requisição</param>
        /// <returns>Entidade LoginResponse com Token e RefreshToken</returns>
        Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress);

        /// <summary>
        /// Método para registrar um novo usuário. Ele recebe uma entidade de RegisterRequest e retorna uma entidade de LoginResponse, que inclui os tokens de acesso e refresh para o usuário recém-registrado.
        /// </summary>
        /// <param name="request">Entidade RegisterRequest</param>
        /// <returns>Retorna o LoginResponse com Token e RefreshToken</returns>
        Task<LoginResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Método para renovar o token de acesso usando um token de refresh. Ele recebe uma entidade de RefreshTokenRequest, que inclui o token de refresh e, opcionalmente, o user agent do cliente. O método retorna uma nova entidade de LoginResponse com um novo token de acesso e um novo token de refresh.
        /// </summary>
        /// <param name="request">Recebe entidade RefreshTokenRequest</param>
        /// <returns>Retorna o LoginResponse com Token e RefreshToken</returns>
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Método para iniciar o processo de recuperação de senha. Ele recebe uma entidade de ForgotPasswordRequest, que inclui o email do usuário que esqueceu a senha. O método retorna um booleano indicando se o processo foi iniciado com sucesso. Normalmente, isso envolveria o envio de um email para o usuário com um link ou token para redefinir a senha.
        /// </summary>
        /// <param name="request">Recebe entidade ForgotPasswordRequest</param>
        /// <returns>Retorna true caso processo funcione, e false se algo der errado</returns>
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);

        /// <summary>
        /// Método para concluir o processo de recuperação de senha. Ele recebe uma entidade de ResetPasswordRequest, que inclui o email do usuário, um código de verificação (geralmente enviado por email) e a nova senha desejada. O método retorna um booleano indicando se a senha foi redefinida com sucesso.
        /// </summary>
        /// <param name="request">Recebe entidade ResetPasswordRequest</param>
        /// <returns>Verdadeiro se for possível resetar senha</returns>
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

        /// <summary>
        /// Método para alterar a senha de um usuário autenticado. Ele recebe o identificador único do usuário (userUid) e uma entidade de ChangePasswordRequest, que inclui a senha atual e a nova senha desejada. O método retorna um booleano indicando se a senha foi alterada com sucesso. Normalmente, isso exigiria que o usuário fornecesse sua senha atual para verificar sua identidade antes de permitir a alteração da senha.
        /// </summary>
        /// <param name="userUid">Recebe o código UID do usuário que deseja trocar senha</param>
        /// <param name="request">Recebe a entidade ChangePasswordRequest</param>
        /// <returns>Verdadeiro se for possível trocar a senha</returns>
        Task<bool> ChangePasswordAsync(Guid userUid, ChangePasswordRequest request);

        /// <summary>
        /// Método para confirmar o email de um usuário. Ele recebe um token de confirmação, que geralmente é enviado para o email do usuário durante o processo de registro. O método retorna um booleano indicando se o email foi confirmado com sucesso. Normalmente, isso envolveria a validação do token e a atualização do status do usuário para indicar que o email foi confirmado.
        /// </summary>
        /// <param name="token">Token recebido pelo usuário</param>
        /// <returns>Verdadeiro se a confirmação puder ser concluída</returns>
        Task<bool> ConfirmEmailAsync(string token);

        /// <summary>
        /// Revoga um refresh token previamente emitido, quando informado.
        /// </summary>
        /// <param name="refreshToken">Refresh token a ser revogado</param>
        Task RevokeRefreshTokenAsync(string? refreshToken);
    }
}
