using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Users.Admin.Responses;

namespace eTasks_server.Client.Services
{
    /// <summary>
    /// Classe para gerenciar as operações administrativas relacionadas aos usuários, como bloqueio, redefinição de senha, confirmação de conta e exclusão. Esta classe atua como um intermediário entre a camada de apresentação (UI) e a camada de lógica de negócios (BLL), facilitando a comunicação e a execução das operações administrativas de forma eficiente e organizada.
    /// </summary>
    /// <param name="_userAdminBLL"></param>
    public class UserAdminService(IUserAdminBLL _userAdminBLL) : IUserAdminService
    {
        /// <summary>
        /// Traz lista de usuários para administração, incluindo informações como bloqueio, confirmação e exclusão.
        /// </summary>
        /// <returns></returns>
        public Task<List<AdminUserDTO>> GetUsersAsync()
        {
            return _userAdminBLL.GetUsersAsync();
        }

        /// <summary>
        /// Bloqueia ou desbloqueia um usuário com base no estado atual. Se o usuário estiver bloqueado, ele será desbloqueado, e vice-versa.
        /// </summary>
        /// <param name="uid">Identificação do usuário</param>
        /// <returns></returns>
        public Task<bool> ToggleBlockAsync(Guid uid)
        {
            return _userAdminBLL.ToggleBlockAsync(uid);
        }

        /// <summary>
        /// Redefine a senha de um usuário para uma nova senha fornecida. Esta operação é útil para administradores que precisam resetar a senha de um usuário ou para usuários que esqueceram suas senhas e precisam de uma nova.
        /// </summary>
        /// <param name="uid">Identificação</param>
        /// <param name="newPassword">Nova senha</param>
        /// <returns></returns>
        public Task<bool> SetPasswordAsync(Guid uid, string newPassword)
        {
            return _userAdminBLL.SetPasswordAsync(uid, newPassword);
        }

        /// <summary>
        /// Confirma a conta de um usuário, marcando-a como verificada. Esta operação é útil para administradores que precisam confirmar manualmente as contas dos usuários ou para usuários que não receberam o email de confirmação e precisam de uma confirmação manual.
        /// </summary>
        /// <param name="uid">Identificação</param>
        /// <returns></returns>
        public Task<bool> ConfirmAccountAsync(Guid uid)
        {
            return _userAdminBLL.ConfirmAccountAsync(uid);
        }

        /// <summary>
        /// Envia um email de redefinição de senha para o usuário especificado. O email conterá um link ou instruções para que o usuário possa criar uma nova senha. Esta operação é útil para administradores que precisam enviar um email de redefinição de senha para um usuário ou para usuários que esqueceram suas senhas e precisam de ajuda para redefini-las.
        /// </summary>
        /// <param name="uid">Identificação do usuário</param>
        /// <returns></returns>
        public Task<bool> SendPasswordResetEmailAsync(Guid uid)
        {
            return _userAdminBLL.SendPasswordResetEmailAsync(uid);
        }

        /// <summary>
        /// Obtem os logs de login de um usuário específico, incluindo informações como status do login, endereço IP, user agent e data/hora do login. Esta operação é útil para administradores que precisam monitorar a atividade de login dos usuários ou para usuários que desejam revisar seus próprios logs de login para fins de segurança.
        /// </summary>
        /// <param name="uid">Identificação do usuário</param>
        /// <returns></returns>
        public Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid)
        {
            return _userAdminBLL.GetLoginLogsAsync(uid);
        }

        /// <summary>
        /// Remove um usuário de forma permanente do sistema. Esta operação é irreversível e excluirá todas as informações associadas ao usuário, incluindo dados de perfil, histórico de atividades e quaisquer outras informações relacionadas. Esta ação é útil para administradores que precisam remover completamente um usuário do sistema, seja por motivos de segurança, privacidade ou outros motivos administrativos.
        /// </summary>
        /// <param name="uid">Identificação do usuário</param>
        /// <returns></returns>
        public Task DeletePermanentlyAsync(Guid uid)
        {
            return _userAdminBLL.DeletePermanentlyAsync(uid);
        }

        /// <summary>
        /// Remove permanentemente todos os usuários que foram marcados como excluídos. Esta operação é útil para administradores que desejam limpar regularmente os usuários excluídos do sistema para manter a base de dados organizada e eficiente. Ao executar esta ação, todos os usuários que foram previamente marcados como excluídos serão removidos de forma permanente, liberando espaço e recursos no sistema.
        /// </summary>
        /// <returns>O número de usuários removidos</returns>
        public Task<int> PurgeDeletedUsersAsync()
        {
            return _userAdminBLL.PurgeDeletedUsersAsync();
        }
    }
}
