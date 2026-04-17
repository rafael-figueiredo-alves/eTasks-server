using eTasks_server.Models.DTOs.Users.Admin.Responses;

namespace eTasks_server.Client.Services.Interfaces
{
    /// <summary>
    /// Interface for user administration services, providing methods for managing user accounts, such as retrieving user lists, blocking/unblocking users, resetting passwords, confirming accounts, and handling login logs. This service is intended for use by administrators to maintain and oversee user accounts effectively.
    /// </summary>
    public interface IUserAdminService
    {
        /// <summary>
        /// Obtem lista de usuários para administração, incluindo detalhes como nome, email, status de confirmação, bloqueio e exclusão, além de informações de criação e último acesso. Esta função é essencial para que os administradores possam visualizar e gerenciar os usuários do sistema de forma eficiente.
        /// </summary>
        /// <returns>Lista de usuários administrativos</returns>
        Task<List<AdminUserDTO>> GetUsersAsync();

        /// <summary>
        /// Bloqueia ou desbloqueia um usuário com base no seu identificador único (UID). Esta função é crucial para os administradores controlarem o acesso dos usuários ao sistema, permitindo bloquear usuários que apresentem comportamento inadequado ou desbloquear aqueles que foram previamente bloqueados.
        /// </summary>
        /// <param name="uid">Identificador único do usuário</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> ToggleBlockAsync(Guid uid);

        /// <summary>
        /// Define uma nova senha para um usuário específico, identificado por seu UID. Esta função é importante para os administradores que precisam redefinir senhas de usuários, seja por motivos de segurança ou para ajudar usuários que esqueceram suas senhas. O administrador pode fornecer uma nova senha diretamente, garantindo que o usuário possa acessar sua conta novamente.
        /// </summary>
        /// <param name="uid">Identificador único do usuário</param>
        /// <param name="newPassword">Nova senha a ser definida</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> SetPasswordAsync(Guid uid, string newPassword);

        /// <summary>
        /// Confirma a conta de um usuário com base em seu UID. Esta função é essencial para os administradores que precisam validar e ativar contas de usuários, especialmente em casos onde a confirmação por email falhou ou quando um administrador precisa aprovar manualmente as contas antes que os usuários possam acessar o sistema. Ao confirmar a conta, o usuário poderá acessar suas funcionalidades normalmente.
        /// </summary>
        /// <param name="uid">Identificador único do usuário</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> ConfirmAccountAsync(Guid uid);

        /// <summary>
        /// Envia um email de redefinição de senha para um usuário específico, identificado por seu UID. Esta função é importante para os administradores que precisam ajudar usuários a recuperar o acesso às suas contas quando eles esquecem suas senhas. Ao enviar o email de redefinição, o usuário receberá instruções para criar uma nova senha e acessar sua conta novamente.
        /// </summary>
        /// <param name="uid">Identificador único do usuário</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> SendPasswordResetEmailAsync(Guid uid);

        /// <summary>
        /// Obtem os logs de login de um usuário específico, identificado por seu UID. Esta função é crucial para os administradores monitorarem a atividade de login dos usuários, permitindo identificar padrões de acesso, detectar possíveis atividades suspeitas e manter um registro histórico das tentativas de login. Os logs incluem informações como status do login, endereço IP, user agent e data/hora do acesso.
        /// </summary>
        /// <param name="uid">Identificador único do usuário</param>
        /// <returns>Lista de logs de login do usuário</returns>
        Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid);

        /// <summary>
        /// Remove um usuário do sistema, marcando-o como excluído. Esta função é importante para os administradores que precisam desativar contas de usuários sem removê-las permanentemente, permitindo que os dados do usuário sejam mantidos para fins de auditoria ou possível reativação no futuro. O usuário marcado como excluído não poderá acessar o sistema, mas seus dados permanecerão armazenados.
        /// </summary>
        /// <param name="uid">Identificador único do usuário</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task DeletePermanentlyAsync(Guid uid);

        /// <summary>
        /// Remove permanentemente os usuários que foram marcados como excluídos há mais de 30 dias. Esta função é essencial para os administradores manterem o banco de dados limpo e eficiente, garantindo que os dados de usuários que não são mais necessários sejam removidos definitivamente após um período de retenção adequado. A função retorna o número de usuários que foram purgados do sistema.
        /// </summary>
        /// <returns>Número de usuários purgados</returns>
        Task<int> PurgeDeletedUsersAsync();
    }
}
