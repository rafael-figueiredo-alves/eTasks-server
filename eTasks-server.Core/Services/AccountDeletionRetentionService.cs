using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.Services
{
    public class AccountDeletionRetentionService(
        AppDbContext context,
        ILogger<IAccountDeletionRetentionService> logger) : IAccountDeletionRetentionService
    {
        public async Task<int> DeleteExpiredAccountsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expiredUserUids = await context.AccountReactivationCodes
                .Where(x => !x.IsUsed && x.ExpiresAt <= now && x.User != null && x.User.IsDeleted)
                .Select(x => x.UserUid)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (expiredUserUids.Count == 0)
            {
                return 0;
            }

            var usersToDelete = await context.Users
                .Where(x => expiredUserUids.Contains(x.Uid) && x.IsDeleted && !x.IsAdmin)
                .ToListAsync(cancellationToken);

            if (usersToDelete.Count == 0)
            {
                return 0;
            }

            context.Users.RemoveRange(usersToDelete);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Removidos permanentemente {Count} usuarios com recuperacao de conta expirada.", usersToDelete.Count);
            return usersToDelete.Count;
        }
    }
}
