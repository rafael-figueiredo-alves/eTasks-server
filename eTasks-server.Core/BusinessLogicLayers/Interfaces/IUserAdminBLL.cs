using eTasks_server.Models.DTOs.Users.Admin.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface de gerenciamento de usuários para administradores, incluindo bloqueio, redefinição de senha e confirmação de conta.
    /// </summary>
    public interface IUserAdminBLL
    {
        /// <summary>
        /// Método para obter a lista de usuários do sistema, retornando informações relevantes para administração, como status de bloqueio e confirmação.
        /// </summary>
        /// <returns>Lista de usuários do sistema eTasks</returns>
        Task<List<AdminUserDTO>> GetUsersAsync();

        /// <summary>
        /// Bloqueia ou desbloqueia um usuário com base no seu status atual. Se o usuário estiver bloqueado, ele será desbloqueado, e vice-versa. O método retorna um booleano indicando se a operação foi bem-sucedida.
        /// </summary>
        /// <param name="uid">UID do usuário a bloquear e/ou desbloquear</param>
        /// <returns>Verdadeiro se operação for bem sucedida</returns>
        Task<bool> ToggleBlockAsync(Guid uid);

        /// <summary>
        /// Definir uma nova senha para um usuário específico, identificado pelo seu UID. O método recebe o UID do usuário e a nova senha, e retorna um booleano indicando se a operação foi bem-sucedida. Esta funcionalidade é essencial para administradores que precisam redefinir senhas de usuários ou configurar senhas iniciais para novos usuários.
        /// </summary>
        /// <param name="uid">UID do usuário a definir nova senha</param>
        /// <param name="newPassword">Nova senha</param>
        /// <returns>Verdadeiro se funcionar</returns>
        Task<bool> SetPasswordAsync(Guid uid, string newPassword);

        /// <summary>
        /// Método para confirmar a conta de um usuário, permitindo que ele acesse o sistema. Esta funcionalidade é crucial para garantir que apenas usuários autorizados possam acessar suas contas, especialmente em casos onde a confirmação de e-mail é necessária para ativar a conta. O método recebe o UID do usuário e retorna um booleano indicando se a operação foi bem-sucedida.
        /// </summary>
        /// <param name="uid">UID do usuário a confirmar conta</param>
        /// <returns>Verdadeiro se operação for bem sucedida</returns>
        Task<bool> ConfirmAccountAsync(Guid uid);

        /// <summary>
        /// Método para enviar um e-mail de redefinição de senha para um usuário específico, identificado pelo seu UID. Esta funcionalidade é essencial para permitir que os usuários recuperem o acesso às suas contas caso esqueçam suas senhas. O método recebe o UID do usuário e retorna um booleano indicando se o e-mail foi enviado com sucesso.
        /// </summary>
        /// <param name="uid">UID do usuário</param>
        /// <returns>Verdadeiro se for bem sucedido</returns>
        Task<bool> SendPasswordResetEmailAsync(Guid uid);

        /// <summary>
        /// Retorna uma lista de logs de login para um usuário específico, identificado pelo seu UID. Esta funcionalidade é importante para administradores monitorarem as atividades de login dos usuários, identificando possíveis tentativas de acesso não autorizadas ou problemas de segurança. O método retorna uma lista de objetos UserLoginLogDTO, que contêm informações sobre cada tentativa de login, como status, endereço IP, user agent e data/hora do login.
        /// </summary>
        /// <param name="uid">UID do usuário a exibir log</param>
        /// <returns>Retorna lista de Logs do usuário</returns>
        Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid);

        /// <summary>
        /// Remove permanentemente um usuário do banco de dados (hard delete).
        /// Todas as entidades relacionadas serão removidas via ON DELETE CASCADE.
        /// Não é permitido remover administradores.
        /// </summary>
        /// <param name="uid">UID do usuário a remover definitivamente</param>
        Task DeletePermanentlyAsync(Guid uid);

        /// <summary>
        /// Remove permanentemente todos os usuários com IsDeleted = true.
        /// </summary>
        /// <returns>Quantidade de contas removidas</returns>
        Task<int> PurgeDeletedUsersAsync();
    }
}
