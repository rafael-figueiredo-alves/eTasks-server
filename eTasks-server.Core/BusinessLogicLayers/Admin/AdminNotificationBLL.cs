using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;
using eTasks_server.Models.Entities.Notifications;
using eTasks_server.Models.Enums.Notifications;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.Admin
{
    /// <summary>
    /// Regras de negocio para envio de notificacoes administrativas aos usuarios do sistema.
    /// </summary>
    public class AdminNotificationBLL(AppDbContext context, ILogger<IAdminNotificationBLL> logger)
        : BaseBLL<IAdminNotificationBLL>(context, logger), IAdminNotificationBLL
    {
        /// <summary>
        /// Envia uma notificacao administrativa para o publico alvo informado.
        /// </summary>
        /// <param name="adminUserUid">Identificador do administrador remetente.</param>
        /// <param name="request">Dados da notificacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resumo do envio realizado.</returns>
        public async Task<SendAdminNotificationResponse> SendAsync(Guid adminUserUid, SendAdminNotificationRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);

            var admin = await GetAndValidateActiveUserAsync(adminUserUid);
            if (!admin.IsAdmin)
            {
                throw new ValidationException(nameof(adminUserUid), "Somente administradores podem enviar notificacoes.");
            }

            var recipients = await ResolveRecipientsAsync(request, cancellationToken);
            if (recipients.Count == 0)
            {
                throw new ValidationException(nameof(request.TargetType), "Nenhum destinatario encontrado para o envio.");
            }

            var now = SaoPauloDateTime.Now();
            var message = new NotificationMessage
            {
                CreatedByUserUid = adminUserUid,
                TargetType = request.TargetType,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                ActionUrl = string.IsNullOrWhiteSpace(request.ActionUrl) ? null : request.ActionUrl.Trim(),
                DataJson = string.IsNullOrWhiteSpace(request.DataJson) ? null : request.DataJson.Trim(),
                CreatedAt = now,
                Recipients = recipients.Select(userUid => new NotificationRecipient
                {
                    UserUid = userUid,
                    CreatedAt = now
                }).ToList()
            };

            await _context.NotificationMessages.AddAsync(message, cancellationToken);
            await SaveChangesContextAsync(cancellationToken);

            var deviceCount = await _context.PushDeviceRegistrations
                .CountAsync(x => recipients.Contains(x.UserUid) && x.IsActive, cancellationToken);

            _logger.LogInformation("Notificacao {NotificationId} criada para {RecipientCount} destinatario(s), com {DeviceCount} dispositivo(s) registrado(s).", message.Id, recipients.Count, deviceCount);

            return new SendAdminNotificationResponse
            {
                NotificationId = message.Id,
                RecipientCount = recipients.Count,
                RegisteredDeviceCount = deviceCount,
                CreatedAt = message.CreatedAt
            };
        }

        /// <summary>
        /// Resolve a lista de usuarios destinatarios da notificacao.
        /// </summary>
        /// <param name="request">Parametros de selecao dos destinatarios.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de identificadores de usuarios destinatarios.</returns>
        private async Task<List<Guid>> ResolveRecipientsAsync(SendAdminNotificationRequest request, CancellationToken cancellationToken)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(x => !x.IsDeleted && !x.IsBlocked && x.IsConfirmed);

            // Aplica o filtro do publico alvo antes de coletar os Uids.
            query = request.TargetType switch
            {
                NotificationTargetType.Admins => query.Where(x => x.IsAdmin),
                NotificationTargetType.RegularUsers => query.Where(x => !x.IsAdmin),
                NotificationTargetType.SelectedUsers => query.Where(x => request.UserUids.Contains(x.Uid)),
                _ => query
            };

            return await query
                .Select(x => x.Uid)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Valida o payload de envio da notificacao.
        /// </summary>
        /// <param name="request">Dados da notificacao.</param>
        private static void ValidateRequest(SendAdminNotificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ValidationException(nameof(request.Title), "Informe o titulo da notificacao.");
            }

            if (request.Title.Length > 120)
            {
                throw new ValidationException(nameof(request.Title), "O titulo deve ter no maximo 120 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                throw new ValidationException(nameof(request.Body), "Informe a mensagem da notificacao.");
            }

            if (request.Body.Length > 500)
            {
                throw new ValidationException(nameof(request.Body), "A mensagem deve ter no maximo 500 caracteres.");
            }

            if (request.TargetType == NotificationTargetType.SelectedUsers && request.UserUids.Count == 0)
            {
                throw new ValidationException(nameof(request.UserUids), "Selecione pelo menos um usuario.");
            }
        }
    }
}
