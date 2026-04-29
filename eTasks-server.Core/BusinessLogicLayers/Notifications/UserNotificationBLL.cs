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
    public class UserNotificationBLL(AppDbContext context, ILogger<IUserNotificationBLL> logger)
        : BaseBLL<IUserNotificationBLL>(context, logger), IUserNotificationBLL
    {
        public async Task<PushDeviceRegistrationResponse> RegisterDeviceAsync(Guid userUid, RegisterPushDeviceRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            if (string.IsNullOrWhiteSpace(request.DeviceId))
            {
                throw new ValidationException(nameof(request.DeviceId), "Informe o identificador do dispositivo.");
            }

            var deviceId = request.DeviceId.Trim();
            var registration = await _context.PushDeviceRegistrations
                .FirstOrDefaultAsync(x => x.UserUid == userUid && x.Platform == request.Platform && x.DeviceId == deviceId, cancellationToken);

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

            registration.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Platform.ToString() : request.DisplayName.Trim();
            registration.PushToken = string.IsNullOrWhiteSpace(request.PushToken) ? null : request.PushToken.Trim();
            registration.PushEndpoint = string.IsNullOrWhiteSpace(request.PushEndpoint) ? null : request.PushEndpoint.Trim();
            registration.P256dh = string.IsNullOrWhiteSpace(request.P256dh) ? null : request.P256dh.Trim();
            registration.Auth = string.IsNullOrWhiteSpace(request.Auth) ? null : request.Auth.Trim();
            registration.IsActive = true;
            registration.UpdatedAt = SaoPauloDateTime.Now();
            registration.LastSeenAt = SaoPauloDateTime.Now();

            await SaveChangesContextAsync(cancellationToken);
            return ToDeviceResponse(registration);
        }

        public async Task<IReadOnlyList<NotificationInboxItemResponse>> GetInboxAsync(Guid userUid, bool unreadOnly = false, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var query = _context.NotificationRecipients
                .AsNoTracking()
                .Include(x => x.Message)
                .Where(x => x.UserUid == userUid);

            if (unreadOnly)
            {
                query = query.Where(x => x.ReadAt == null);
            }

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

        public async Task<int> GetUnreadCountAsync(Guid userUid, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            return await _context.NotificationRecipients.CountAsync(x => x.UserUid == userUid && x.ReadAt == null, cancellationToken);
        }

        public async Task MarkAsReadAsync(Guid userUid, Guid recipientId, CancellationToken cancellationToken = default)
        {
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(x => x.Id == recipientId && x.UserUid == userUid, cancellationToken);

            if (recipient is null)
            {
                throw new ApiException(HttpStatusCode.NotFound, "Notificacao nao encontrada.");
            }

            recipient.ReadAt ??= SaoPauloDateTime.Now();
            await SaveChangesContextAsync(cancellationToken);
        }

        public async Task MarkAllAsReadAsync(Guid userUid, CancellationToken cancellationToken = default)
        {
            var recipients = await _context.NotificationRecipients
                .Where(x => x.UserUid == userUid && x.ReadAt == null)
                .ToListAsync(cancellationToken);

            foreach (var recipient in recipients)
            {
                recipient.ReadAt = SaoPauloDateTime.Now();
            }

            await SaveChangesContextAsync(cancellationToken);
        }

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
    }
}
