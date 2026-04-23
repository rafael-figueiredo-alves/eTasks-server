using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Notes.Requests;
using eTasks_server.Models.DTOs.Notes.Responses;
using eTasks_server.Models.Entities.Notes;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Notes
{
    public class NoteBLL(AppDbContext context, ILogger<INoteBLL> logger) : BaseBLL<INoteBLL>(context, logger), INoteBLL
    {
        public async Task<List<NoteListItemResponse>> ListAsync(Guid userUid, ListNotesRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            NormalizeListRequest(request);
            ValidateListRequest(request);

            var query = _context.Notes
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(x => x.Subject.Contains(searchTerm) || x.Content.Contains(searchTerm));
            }

            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= request.CreatedTo.Value);
            }

            if (request.UpdatedFrom.HasValue)
            {
                query = query.Where(x => x.UpdatedAt.HasValue && x.UpdatedAt.Value >= request.UpdatedFrom.Value);
            }

            if (request.UpdatedTo.HasValue)
            {
                query = query.Where(x => x.UpdatedAt.HasValue && x.UpdatedAt.Value <= request.UpdatedTo.Value);
            }

            return await query
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ThenBy(x => x.Subject)
                .Select(x => new NoteListItemResponse
                {
                    Id = x.Id,
                    Subject = x.Subject,
                    Preview = BuildPreview(x.Content),
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<NoteDetailsResponse> GetByIdAsync(Guid userUid, Guid noteId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var note = await _context.Notes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == noteId && !x.IsDeleted, cancellationToken);

            note = EnsureFound(note, "Anotacao nao encontrada.");
            EnsureOwnership(note.UserUid, userUid);

            return MapDetails(note);
        }

        public async Task<NoteDetailsResponse> CreateAsync(Guid userUid, CreateNoteRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Subject, request.Content);
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            var note = new NoteItem
            {
                Id = request.ClientGeneratedId ?? Guid.CreateVersion7(),
                UserUid = userUid,
                Subject = request.Subject.Trim(),
                Content = request.Content.Trim(),
                CreatedAt = SaoPauloDateTime.Now(),
                UpdatedAt = null
            };

            await _context.Notes.AddAsync(note, cancellationToken);
            await SaveChangesContextAsync(cancellationToken);

            return await GetByIdAsync(userUid, note.Id, cancellationToken);
        }

        public async Task<NoteDetailsResponse> UpdateAsync(Guid userUid, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Subject, request.Content);

            var note = await _context.Notes
                .FirstOrDefaultAsync(x => x.Id == noteId && !x.IsDeleted, cancellationToken);

            note = EnsureFound(note, "Anotacao nao encontrada.");
            EnsureOwnership(note.UserUid, userUid);

            note.Subject = request.Subject.Trim();
            note.Content = request.Content.Trim();
            note.UpdatedAt = SaoPauloDateTime.Now();

            await SaveChangesContextAsync(cancellationToken);

            return await GetByIdAsync(userUid, note.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid userUid, Guid noteId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var note = await _context.Notes
                .FirstOrDefaultAsync(x => x.Id == noteId && !x.IsDeleted, cancellationToken);

            note = EnsureFound(note, "Anotacao nao encontrada.");
            EnsureOwnership(note.UserUid, userUid);

            var deletedAt = SaoPauloDateTime.Now();
            note.IsDeleted = true;
            note.DeletedAt = deletedAt;
            note.UpdatedAt = deletedAt;

            await SaveChangesContextAsync(cancellationToken);
        }

        public async Task<NoteSyncResponse> SyncAsync(Guid userUid, SyncNotesRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var since = request.Since;
            var serverTime = SaoPauloDateTime.Now();

            var upsertsQuery = _context.Notes
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            var deletedQuery = _context.Notes
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            if (since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > since.Value);
            }

            var upserts = await upsertsQuery
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            var deleted = await deletedQuery
                .OrderBy(x => x.DeletedAt)
                .ThenBy(x => x.Id)
                .Select(x => new DeletedNoteResponse
                {
                    Id = x.Id,
                    DeletedAt = x.DeletedAt!.Value
                })
                .ToListAsync(cancellationToken);

            return new NoteSyncResponse
            {
                ServerTime = serverTime,
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }

        private static void NormalizeListRequest(ListNotesRequest request)
        {
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        private static void ValidateListRequest(ListNotesRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchTerm) && request.SearchTerm.Trim().Length > 200)
            {
                throw new ValidationException("SearchTerm", "O termo de pesquisa deve ter no maximo 200 caracteres.");
            }

            ValidateDateRange(request.CreatedFrom, request.CreatedTo, "CreatedTo");
            ValidateDateRange(request.UpdatedFrom, request.UpdatedTo, "UpdatedTo");
        }

        private static void ValidateDateRange(DateTime? from, DateTime? to, string fieldName)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                throw new ValidationException(fieldName, "A data final deve ser maior ou igual a data inicial.");
            }
        }

        private static void ValidatePayload(string subject, string content)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ValidationException("Subject", "O assunto da anotacao e obrigatorio.");
            }

            if (subject.Trim().Length > 200)
            {
                throw new ValidationException("Subject", "O assunto da anotacao deve ter no maximo 200 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ValidationException("Content", "O conteudo da anotacao e obrigatorio.");
            }

            if (content.Trim().Length > 20000)
            {
                throw new ValidationException("Content", "O conteudo da anotacao deve ter no maximo 20000 caracteres.");
            }
        }

        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            var alreadyExists = await _context.Notes.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Ja existe uma anotacao com o identificador informado pelo cliente.");
            }
        }

        private static string BuildPreview(string content)
        {
            var normalized = string.Join(' ', content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            if (normalized.Length <= 180)
            {
                return normalized;
            }

            return $"{normalized[..177]}...";
        }

        private static NoteDetailsResponse MapDetails(NoteItem note)
        {
            return new NoteDetailsResponse
            {
                Id = note.Id,
                UserUid = note.UserUid,
                Subject = note.Subject,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }
}
