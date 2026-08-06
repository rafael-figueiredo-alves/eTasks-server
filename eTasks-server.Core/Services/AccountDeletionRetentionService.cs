using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Servi~ço responsável por gerenciar a retenção e exclusão de contas expiradas.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    public class AccountDeletionRetentionService(
        AppDbContext context,
        ILogger<IAccountDeletionRetentionService> logger) : IAccountDeletionRetentionService
    {
        /// <summary>
        /// Apaga permanentemente contas de usuários que possuem códigos de reativação expirados e que foram marcados como excluídos.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> DeleteExpiredAccountsAsync(CancellationToken cancellationToken = default)
        {
            // Obtém a data e hora atual em UTC
            var now = DateTime.UtcNow;

            // Seleciona os UIDs dos usuários que possuem códigos de reativação expirados e que foram marcados como excluídos
            var expiredUserUids = await context.AccountReactivationCodes
                .Where(x => !x.IsUsed && x.ExpiresAt <= now && x.User != null && x.User.IsDeleted)
                .Select(x => x.UserUid)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Se não houver usuários expirados, retorna 0
            if (expiredUserUids.Count == 0)
            {
                return 0;
            }

            // Seleciona os usuários que possuem UIDs expirados, que foram marcados como excluídos e que não são administradores
            var usersToDelete = await context.Users
                .Where(x => expiredUserUids.Contains(x.Uid) && x.IsDeleted && !x.IsAdmin)
                .ToListAsync(cancellationToken);

            // Se não houver usuários a serem deletados, retorna 0
            if (usersToDelete.Count == 0)
            {
                return 0;
            }

            // Remove os usuários selecionados do contexto e salva as alterações no banco de dados
            context.Users.RemoveRange(usersToDelete);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Removidos permanentemente {Count} usuários com recuperação de conta expirada.", usersToDelete.Count);
            return usersToDelete.Count;
        }
    }
}
