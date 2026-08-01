using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.Users.Admin.Responses;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace eTasks_server.Core.BusinessLogicLayers.Admin
{
    /// <summary>
    /// Regras de negocio para administracao de usuarios no painel.
    /// </summary>
    public class UserAdminBLL : BaseBLL<IUserAdminBLL>, IUserAdminBLL
    {
        /// <summary>
        /// Serviço de envio de e-mails.
        /// </summary>
        private readonly IEmailService _emailService;

        /// <summary>
        /// Serviço de proteção de segredos (criptografia).
        /// </summary>
        private readonly ISecretProtector _secretProtector;

        /// <summary>
        /// Método construtor da classe UserAdminBLL.
        /// </summary>
        /// <param name="context">Contexto de dados</param>
        /// <param name="emailService">Serviço de envio de e-mails</param>
        /// <param name="secretProtector">Serviço de proteção de segredos</param>
        /// <param name="logger">Logger</param>
        public UserAdminBLL(AppDbContext context, IEmailService emailService, ISecretProtector secretProtector, ILogger<IUserAdminBLL> logger) : base(context, logger)
        {
            _emailService = emailService;
            _secretProtector = secretProtector;
        }

        /// <summary>
        /// Retorna a lista de usuários não administrativos ativos.
        /// </summary>
        /// <returns>Lista de usuários para o painel.</returns>
        public async Task<List<AdminUserDTO>> GetUsersAsync()
        {
            // Retorna todos os usuários que não são administradores e não estão marcados como excluídos, ordenados pela data de criação.
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

        /// <summary>
        /// Alterna o bloqueio de um usuario.
        /// </summary>
        /// <param name="uid">Identificador do usuario.</param>
        /// <returns>True quando a operacao for concluida.</returns>
        public async Task<bool> ToggleBlockAsync(Guid uid)
        {
            // Busca o usuário pelo UID, garantindo que ele não esteja marcado como excluído.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);

            // Se o usuário não for encontrado, lança uma exceção de API com status 404.
            if (user == null) throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");

            // Se o usuário for um administrador, lança uma exceção de API com status 403.
            if (user.IsAdmin) throw new ApiException(HttpStatusCode.Forbidden, "Não é possível bloquear um administrador.");

            // Alterna o estado de bloqueio do usuário.
            user.IsBlocked = !user.IsBlocked;

            // Se o usuário estiver bloqueado, revoga todos os tokens de atualização ativos.
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

        /// <summary>
        /// Define uma nova senha para o usuario.
        /// </summary>
        /// <param name="uid">Identificador do usuario.</param>
        /// <param name="newPassword">Nova senha.</param>
        /// <returns>True quando a operacao for concluida.</returns>
        public async Task<bool> SetPasswordAsync(Guid uid, string newPassword)
        {
            // Busca o usuário pelo UID, garantindo que ele não esteja marcado como excluído.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);

            // Se o usuário não for encontrado, lança uma exceção de API com status 404.
            if (user == null) throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");

            // Se o usuário for um administrador, lança uma exceção de API com status 403.
            if (user.IsAdmin) throw new ApiException(HttpStatusCode.Forbidden, "Não é possível alterar a senha de um administrador.");

            // Atualiza a senha do usuário, protegendo-a com o serviço de proteção de segredos.
            user.PasswordHash = _secretProtector.Protect(BCrypt.Net.BCrypt.HashPassword(newPassword));

            // Revoga tokens para forçar novo login com a nova senha
            var tokens = await _context.RefreshTokens.Where(t => t.UserUid == uid && !t.IsRevoked).ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Marca a conta do usuario como confirmada.
        /// </summary>
        /// <param name="uid">Identificador do usuario.</param>
        /// <returns>True quando a operacao for concluida.</returns>
        public async Task<bool> ConfirmAccountAsync(Guid uid)
        {
            // Busca o usuário pelo UID, garantindo que ele não esteja marcado como excluído.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);

            // Se o usuário não for encontrado, lança uma exceção de API com status 404.
            if (user == null) throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");

            // Se o usuário for um administrador, lança uma exceção de API com status 403.
            if (user.IsAdmin) throw new ApiException(HttpStatusCode.Forbidden, "Não é possível confirmar a conta de um administrador.");

            user.IsConfirmed = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Gera e envia um e-mail de redefinicao de senha.
        /// </summary>
        /// <param name="uid">Identificador do usuario.</param>
        /// <returns>True quando a operacao for concluida.</returns>
        public async Task<bool> SendPasswordResetEmailAsync(Guid uid)
        {
            // Busca o usuário pelo UID, garantindo que ele não esteja marcado como excluído.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid && !u.IsDeleted);

            // Se o usuário não for encontrado, lança uma exceção de API com status 404.
            if (user == null) throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");

            // Se o usuário for um administrador, lança uma exceção de API com status 403.
            if (user.IsAdmin) throw new ApiException(HttpStatusCode.Forbidden, "Não é possível redefinir a senha de um administrador.");

            // Gera um código de redefinição de senha aleatório de 6 dígitos.
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();

            // Cria um novo registro de código de redefinição de senha com validade de 15 minutos.
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

        /// <summary>
        /// Retorna os ultimos logs de login de um usuario.
        /// </summary>
        /// <param name="uid">Identificador do usuario.</param>
        /// <returns>Lista dos ultimos logs de login.</returns>
        public async Task<List<UserLoginLogDTO>> GetLoginLogsAsync(Guid uid)
        {
            // Busca o usuário pelo UID, garantindo que ele não esteja marcado como excluído.
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

        /// <summary>
        /// Remove permanentemente uma conta de usuario.
        /// </summary>
        /// <param name="uid">Identificador do usuario.</param>
        public async Task DeletePermanentlyAsync(Guid uid)
        {
            // Busca o usuário pelo UID.
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Uid == uid);

            // Se o usuário não for encontrado, lança uma exceção de API com status 404.
            if (user is null)
                throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");

            // Se o usuário for um administrador, lança uma exceção de API com status 403.
            if (user.IsAdmin)
                throw new ApiException(HttpStatusCode.Forbidden, "Não é permitido remover contas administrativas.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove em lote usuarios marcados como excluidos.
        /// </summary>
        /// <returns>Quantidade de usuarios removidos.</returns>
        public async Task<int> PurgeDeletedUsersAsync()
        {
            // Busca todos os usuários que estão marcados como excluídos e não são administradores.
            var deletedUsers = await _context.Users
                .Where(u => u.IsDeleted && !u.IsAdmin)
                .ToListAsync();

            // Se não houver usuários excluídos, retorna 0.
            if (deletedUsers.Count == 0)
                return 0;

            _context.Users.RemoveRange(deletedUsers);
            await _context.SaveChangesAsync();
            return deletedUsers.Count;
        }
    }
}
