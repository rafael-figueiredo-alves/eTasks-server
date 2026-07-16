using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Notifications.Requests;
using eTasks_server.Models.DTOs.Notifications.Responses;
using eTasks_server.Models.Entities.Notifications;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace eTasks_server.Core.BusinessLogicLayers.Notifications
{
    /// <summary>
    /// Classe de negócio para gerenciar notificações do usuário
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    public class UserNotificationBLL(AppDbContext context, ILogger<IUserNotificationBLL> logger)
        : BaseBLL<IUserNotificationBLL>(context, logger), IUserNotificationBLL
    {
        #region Funções principais
        /// <summary>
        /// Método que registro o dispositivo para receber mensagens
        /// </summary>
        /// <param name="userUid">ID do usuário</param>
        /// <param name="request">dados do dispositivo</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<PushDeviceRegistrationResponse> RegisterDeviceAsync(Guid userUid, RegisterPushDeviceRequest request, CancellationToken cancellationToken = default)
        {
            // Valida se usuário informado é usuário ativo e válido, não bloqueado
            await GetAndValidateActiveUserAsync(userUid);

            // Valida se o id do dispositivo está vazio ou em branco
            if (string.IsNullOrWhiteSpace(request.DeviceId))
            {
                throw new ValidationException(nameof(request.DeviceId), "Informe o identificador do dispositivo.");
            }

            // Grava o id do dispositivo
            var deviceId = request.DeviceId.Trim();

            // Obtem registro do usuário, plataforma e id do dispositivo
            var registration = await _context.PushDeviceRegistrations
                .FirstOrDefaultAsync(x => x.UserUid == userUid && x.Platform == request.Platform && x.DeviceId == deviceId, cancellationToken);

            // se registro vazio, gera novo registro
            if (registration is null)
            {
                registration = new PushDeviceRegistration
                {
                    UserUid = userUid,
                    Platform = request.Platform,
                    DeviceId = deviceId,
                    CreatedAt = SaoPauloDateTime.Now()
                };
                await _context.PushDeviceRegistrations.AddAsync(registration, cancellationToken);
            }

            // se não, atualiza as informações
            registration.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Platform.ToString() : request.DisplayName.Trim();
            registration.PushToken = string.IsNullOrWhiteSpace(request.PushToken) ? null : request.PushToken.Trim();
            registration.PushEndpoint = string.IsNullOrWhiteSpace(request.PushEndpoint) ? null : request.PushEndpoint.Trim();
            registration.P256dh = string.IsNullOrWhiteSpace(request.P256dh) ? null : request.P256dh.Trim();
            registration.Auth = string.IsNullOrWhiteSpace(request.Auth) ? null : request.Auth.Trim();
            registration.IsActive = true;
            registration.UpdatedAt = SaoPauloDateTime.Now();
            registration.LastSeenAt = SaoPauloDateTime.Now();

            // Salva dados
            await SaveChangesContextAsync(cancellationToken);

            // Gera o registro como resposta
            return ToDeviceResponse(registration);
        }

        /// <summary>
        /// Retorna mensagens da caixa de notificações
        /// </summary>
        /// <param name="userUid">ID do usuário</param>
        /// <param name="unreadOnly">apenas listar não lidas</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<NotificationInboxItemResponse>> GetInboxAsync(Guid userUid, bool unreadOnly = false, CancellationToken cancellationToken = default)
        {
            // Valida se usuário informado é usuário ativo e válido, não bloqueado
            await GetAndValidateActiveUserAsync(userUid);

            // Pega as notificações do usuário informado
            var query = _context.NotificationRecipients
                .AsNoTracking()
                .Include(x => x.Message)
                .Where(x => x.UserUid == userUid);

            // Se tiver pedido apenas as não lidas, filtra apenas as sem data de leitura
            if (unreadOnly)
            {
                query = query.Where(x => x.ReadAt == null);
            }

            // Retorna lista com as 100 últimas notificações encontradas
            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(100)
                .Select(x => new NotificationInboxItemResponse
                {
                    RecipientId = x.Id,
                    NotificationId = x.NotificationMessageId,
                    Title = x.Message!.Title,
                    Body = x.Message.Body,
                    ActionUrl = x.Message.ActionUrl,
                    DataJson = x.Message.DataJson,
                    CreatedAt = x.Message.CreatedAt,
                    ReadAt = x.ReadAt
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retorna quantidade de mensagens/notificações não lidas de um usuário
        /// </summary>
        /// <param name="userUid">Id do usuário</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> GetUnreadCountAsync(Guid userUid, CancellationToken cancellationToken = default)
        {
            // Valida se usuário informado é usuário ativo e válido, não bloqueado
            await GetAndValidateActiveUserAsync(userUid);

            // retorna quantidade de notificações não lidas do usuário
            return await _context.NotificationRecipients.CountAsync(x => x.UserUid == userUid && x.ReadAt == null, cancellationToken);
        }

        /// <summary>
        /// Marca a notificação atual como lida
        /// </summary>
        /// <param name="userUid">Id do usuário</param>
        /// <param name="recipientId">Id da notificação</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task MarkAsReadAsync(Guid userUid, Guid recipientId, CancellationToken cancellationToken = default)
        {
            // Pega a notifica~ção especificada
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(x => x.Id == recipientId && x.UserUid == userUid, cancellationToken);

            // Se não encontrar, retorna 404, não encontrada
            if (recipient is null)
            {
                throw new ApiException(HttpStatusCode.NotFound, "Notificação não encontrada.");
            }

            // Se não possuir data de leitura, define como a atual
            recipient.ReadAt ??= SaoPauloDateTime.Now();

            // Salva tudo
            await SaveChangesContextAsync(cancellationToken);
        }

        /// <summary>
        /// MArca todas as mensagens como lidas
        /// </summary>
        /// <param name="userUid">Id do usuário</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task MarkAllAsReadAsync(Guid userUid, CancellationToken cancellationToken = default)
        {
            // Pega todas as notificações não lidas do usuário informado
            var recipients = await _context.NotificationRecipients
                .Where(x => x.UserUid == userUid && x.ReadAt == null)
                .ToListAsync(cancellationToken);

            // registra a data de leitura como sendo a data de hoje
            foreach (var recipient in recipients)
            {
                recipient.ReadAt = SaoPauloDateTime.Now();
            }

            // Salva as alterações
            await SaveChangesContextAsync(cancellationToken);
        }
        #endregion

        #region Método particular
        /// <summary>
        /// Retorna dados do dispositivo conectado, como se está ativo, nome e último login
        /// </summary>
        /// <param name="registration">Dados do registro</param>
        /// <returns></returns>
        private static PushDeviceRegistrationResponse ToDeviceResponse(PushDeviceRegistration registration)
            => new()
            {
                Id = registration.Id,
                Platform = registration.Platform,
                DeviceId = registration.DeviceId,
                DisplayName = registration.DisplayName,
                IsActive = registration.IsActive,
                LastSeenAt = registration.LastSeenAt
            };
        #endregion
    }
}
