using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Users.Admin.Responses;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace eTasks_server.Core.BusinessLogicLayers
{
    public class UserAdminBLL : BaseBLL<IUserAdminBLL>, IUserAdminBLL
    {
        private readonly IEmailService _emailService;

        public UserAdminBLL(AppDbContext context, IEmailService emailService, ILogger<IUserAdminBLL> logger) : base(context, logger)
        {
            _emailService = emailService;
        }

        public async Task<List<AdminUserDTO>> GetUsersAsync()
        {
            return await _context.Users
                .Where(u => !u.IsAdmin && !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserDTO
                {
                    Uid = u.Uid,
                    Name = u.Name,
                    Email = u.Email,
                    PhotoPath = u.PhotoPath,
                    IsConfirmed = u.IsConfirmed,
                    IsBlocked = u.IsBlocked,
                    IsDeleted = u.IsDeleted,
                    CreatedAt = u.CreatedAt,
                    LastAccessAt = u.LastAccessAt
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleBlockAsync(Guid uid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);
            if (user == null) throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");
            if (user.IsAdmin) throw new ApiException(HttpStatusCode.Forbidden, "Não é possível bloquear um administrador.");

            user.IsBlocked = !user.IsBlocked;

            if (user.IsBlocked)
            {
                // Revoga todos os tokens se o usuário for bloqueado
                var tokens = await _context.RefreshTokens.Where(t => t.UserUid == uid && !t.IsRevoked).ToListAsync();
                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPasswordAsync(Guid uid, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);
            if (user == null) throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Revoga tokens para forçar novo login com a nova senha
            var tokens = await _context.RefreshTokens.Where(t => t.UserUid == uid && !t.IsRevoked).ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmAccountAsync(Guid uid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);
            if (user == null) throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");

            user.IsConfirmed = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendPasswordResetEmailAsync(Guid uid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);
            if (user == null) throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");

            var random = new Random();
            string code = random.Next(100000, 999999).ToString();

            var resetCode = new PasswordResetCode
            {
                UserUid = user.Uid,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            await _context.PasswordResetCodes.AddAsync(resetCode);
            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, code);
            return true;
        }

        public async Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid)
        {
            return await _context.LoginLogs
                .Where(l => l.UserUid == uid)
                .OrderByDescending(l => l.CreatedAt)
                .Take(50)
                .Select(l => new UserLoginLogDTO
                {
                    Id = l.Id,
                    Status = l.Status,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();
        }

        public async Task DeletePermanentlyAsync(Guid uid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid);
            if (user is null)
                throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");
            if (user.IsAdmin)
                throw new ApiException(HttpStatusCode.Forbidden, "Não é permitido remover contas administrativas.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<int> PurgeDeletedUsersAsync()
        {
            var deletedUsers = await _context.Users
                .Where(u => u.IsDeleted && !u.IsAdmin)
                .ToListAsync();

            if (deletedUsers.Count == 0)
                return 0;

            _context.Users.RemoveRange(deletedUsers);
            await _context.SaveChangesAsync();
            return deletedUsers.Count;
        }
    }
}
