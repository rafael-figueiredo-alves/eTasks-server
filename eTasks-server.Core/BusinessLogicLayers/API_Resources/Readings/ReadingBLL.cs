using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Readings.Requests;
using eTasks_server.Models.DTOs.Readings.Responses;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Entities.Readings;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Readings
{
    public class ReadingBLL(AppDbContext context, ILogger<IReadingBLL> logger) : BaseBLL<IReadingBLL>(context, logger), IReadingBLL
    {
        public async Task<List<ReadingListItemResponse>> ListAsync(Guid userUid, ListReadingsRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            NormalizeListRequest(request);
            ValidateListRequest(request);

            var query = _context.ReadingItems
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            if (request.Format.HasValue)
            {
                query = query.Where(x => x.Format == request.Format.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Genre))
            {
                query = query.Where(x => x.Genre != null && x.Genre.Contains(request.Genre));
            }

            if (request.RatingFrom.HasValue)
            {
                query = query.Where(x => x.Rating.HasValue && x.Rating.Value >= request.RatingFrom.Value);
            }

            if (request.RatingTo.HasValue)
            {
                query = query.Where(x => x.Rating.HasValue && x.Rating.Value <= request.RatingTo.Value);
            }

            if (request.StartedFrom.HasValue)
            {
                query = query.Where(x => x.StartedAt.HasValue && x.StartedAt.Value >= request.StartedFrom.Value);
            }

            if (request.StartedTo.HasValue)
            {
                query = query.Where(x => x.StartedAt.HasValue && x.StartedAt.Value <= request.StartedTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Title.Contains(searchTerm) ||
                    (x.Authors != null && x.Authors.Contains(searchTerm)) ||
                    (x.Subject != null && x.Subject.Contains(searchTerm)) ||
                    (x.Summary != null && x.Summary.Contains(searchTerm)));
            }

            return await query
                .OrderBy(x => x.Status)
                .ThenBy(x => x.Title)
                .Select(x => new ReadingListItemResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Authors = x.Authors,
                    Subject = x.Subject,
                    Genre = x.Genre,
                    Rating = x.Rating,
                    TotalPages = x.TotalPages,
                    CurrentPage = x.CurrentPage,
                    ProgressPercent = x.TotalPages <= 0 ? 0 : Math.Round((decimal)x.CurrentPage * 100m / x.TotalPages, 2),
                    Format = x.Format,
                    Status = x.Status,
                    StartedAt = x.StartedAt,
                    FinishedAt = x.FinishedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<ReadingDetailsResponse> GetByIdAsync(Guid userUid, Guid readingId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var reading = await _context.ReadingItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDeleted, cancellationToken);

            reading = EnsureFound(reading, "Leitura nao encontrada.");
            EnsureOwnership(reading.UserUid, userUid);

            return MapDetails(reading);
        }

        public async Task<ReadingDetailsResponse> CreateAsync(Guid userUid, CreateReadingRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Title, request.Authors, request.Subject, request.Summary, request.Opinion, request.Rating, request.TotalPages, request.CurrentPage, request.Format, request.Status, request.StartedAt, request.FinishedAt);
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            var reading = new ReadingItem
            {
                Id = request.ClientGeneratedId ?? Guid.CreateVersion7(),
                UserUid = userUid,
                Title = request.Title.Trim(),
                Authors = NormalizeText(request.Authors),
                Subject = NormalizeText(request.Subject),
                Summary = NormalizeLongText(request.Summary),
                Opinion = NormalizeLongText(request.Opinion),
                Rating = request.Rating,
                TotalPages = request.TotalPages,
                CurrentPage = request.CurrentPage,
                Genre = NormalizeText(request.Genre),
                Format = request.Format,
                Status = request.Status,
                StartedAt = request.StartedAt,
                FinishedAt = request.FinishedAt,
                CreatedAt = SaoPauloDateTime.Now()
            };

            NormalizeProgressState(reading);

            await ExecuteInTransactionAsync(async () =>
            {
                await _context.ReadingItems.AddAsync(reading, cancellationToken);
                await SaveChangesContextAsync(cancellationToken);

                if (reading.Status == ReadingStatus.Completed)
                {
                    await AwardCompletionPointsAsync(reading, cancellationToken);
                    await SaveChangesContextAsync(cancellationToken);
                }
            });

            return await GetByIdAsync(userUid, reading.Id, cancellationToken);
        }

        public async Task<ReadingDetailsResponse> UpdateAsync(Guid userUid, Guid readingId, UpdateReadingRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Title, request.Authors, request.Subject, request.Summary, request.Opinion, request.Rating, request.TotalPages, request.CurrentPage, request.Format, request.Status, request.StartedAt, request.FinishedAt);

            var reading = await _context.ReadingItems.FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDeleted, cancellationToken);
            reading = EnsureFound(reading, "Leitura nao encontrada.");
            EnsureOwnership(reading.UserUid, userUid);

            var wasCompleted = reading.Status == ReadingStatus.Completed;

            reading.Title = request.Title.Trim();
            reading.Authors = NormalizeText(request.Authors);
            reading.Subject = NormalizeText(request.Subject);
            reading.Summary = NormalizeLongText(request.Summary);
            reading.Opinion = NormalizeLongText(request.Opinion);
            reading.Rating = request.Rating;
            reading.TotalPages = request.TotalPages;
            reading.CurrentPage = request.CurrentPage;
            reading.Genre = NormalizeText(request.Genre);
            reading.Format = request.Format;
            reading.Status = request.Status;
            reading.StartedAt = request.StartedAt;
            reading.FinishedAt = request.FinishedAt;
            reading.UpdatedAt = SaoPauloDateTime.Now();

            NormalizeProgressState(reading);

            await ExecuteInTransactionAsync(async () =>
            {
                if (!wasCompleted && reading.Status == ReadingStatus.Completed)
                {
                    await AwardCompletionPointsAsync(reading, cancellationToken);
                }
                else if (wasCompleted && reading.Status != ReadingStatus.Completed)
                {
                    await RevertCompletionPointsAsync(reading, cancellationToken);
                }

                await SaveChangesContextAsync(cancellationToken);
            });

            return await GetByIdAsync(userUid, reading.Id, cancellationToken);
        }

        public async Task<ReadingDetailsResponse> UpdateProgressAsync(Guid userUid, Guid readingId, UpdateReadingProgressRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var reading = await _context.ReadingItems.FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDeleted, cancellationToken);
            reading = EnsureFound(reading, "Leitura nao encontrada.");
            EnsureOwnership(reading.UserUid, userUid);

            if (request.CurrentPage < 0 || request.CurrentPage > reading.TotalPages)
            {
                throw new ValidationException("CurrentPage", "A pagina atual deve estar entre zero e o total de paginas.");
            }

            var wasCompleted = reading.Status == ReadingStatus.Completed;

            reading.CurrentPage = request.CurrentPage;
            reading.UpdatedAt = SaoPauloDateTime.Now();

            if (reading.CurrentPage == 0)
            {
                reading.Status = ReadingStatus.ToRead;
                reading.StartedAt = null;
                reading.FinishedAt = null;
            }
            else if (reading.CurrentPage >= reading.TotalPages)
            {
                reading.Status = ReadingStatus.Completed;
                reading.StartedAt ??= SaoPauloDateTime.Now();
                reading.FinishedAt ??= SaoPauloDateTime.Now();
            }
            else
            {
                reading.Status = ReadingStatus.Reading;
                reading.StartedAt ??= SaoPauloDateTime.Now();
                reading.FinishedAt = null;
            }

            await ExecuteInTransactionAsync(async () =>
            {
                if (!wasCompleted && reading.Status == ReadingStatus.Completed)
                {
                    await AwardCompletionPointsAsync(reading, cancellationToken);
                }
                else if (wasCompleted && reading.Status != ReadingStatus.Completed)
                {
                    await RevertCompletionPointsAsync(reading, cancellationToken);
                }

                await SaveChangesContextAsync(cancellationToken);
            });

            return await GetByIdAsync(userUid, reading.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid userUid, Guid readingId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var reading = await _context.ReadingItems.FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDeleted, cancellationToken);
            reading = EnsureFound(reading, "Leitura nao encontrada.");
            EnsureOwnership(reading.UserUid, userUid);

            await ExecuteInTransactionAsync(async () =>
            {
                await RevertCompletionPointsAsync(reading, cancellationToken);
                var deletedAt = SaoPauloDateTime.Now();
                reading.IsDeleted = true;
                reading.DeletedAt = deletedAt;
                reading.UpdatedAt = deletedAt;
                await SaveChangesContextAsync(cancellationToken);
            });
        }

        public async Task<ReadingSyncResponse> SyncAsync(Guid userUid, SyncReadingsRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var upsertsQuery = _context.ReadingItems.AsNoTracking().Where(x => x.UserUid == userUid && !x.IsDeleted);
            var deletedQuery = _context.ReadingItems.AsNoTracking().Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            if (request.Since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > request.Since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > request.Since.Value);
            }

            var upserts = await upsertsQuery.OrderBy(x => x.Title).ThenBy(x => x.Id).ToListAsync(cancellationToken);
            var deleted = await deletedQuery.OrderBy(x => x.DeletedAt).ThenBy(x => x.Id)
                .Select(x => new DeletedReadingResponse { Id = x.Id, DeletedAt = x.DeletedAt!.Value })
                .ToListAsync(cancellationToken);

            return new ReadingSyncResponse
            {
                ServerTime = SaoPauloDateTime.Now(),
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }

        private static void NormalizeListRequest(ListReadingsRequest request)
        {
            request.Genre = string.IsNullOrWhiteSpace(request.Genre) ? null : request.Genre.Trim();
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        private static void ValidateListRequest(ListReadingsRequest request)
        {
            if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
            {
                throw new ValidationException("Status", "Status da leitura invalido.");
            }

            if (request.Format.HasValue && !Enum.IsDefined(request.Format.Value))
            {
                throw new ValidationException("Format", "Formato da leitura invalido.");
            }

            if (request.RatingFrom.HasValue && (request.RatingFrom.Value < 0 || request.RatingFrom.Value > 5))
            {
                throw new ValidationException("RatingFrom", "A avaliacao minima deve estar entre zero e cinco.");
            }

            if (request.RatingTo.HasValue && (request.RatingTo.Value < 0 || request.RatingTo.Value > 5))
            {
                throw new ValidationException("RatingTo", "A avaliacao maxima deve estar entre zero e cinco.");
            }

            if (request.RatingFrom.HasValue && request.RatingTo.HasValue && request.RatingFrom.Value > request.RatingTo.Value)
            {
                throw new ValidationException("RatingTo", "A avaliacao maxima deve ser maior ou igual a minima.");
            }

            if (request.StartedFrom.HasValue && request.StartedTo.HasValue && request.StartedFrom.Value > request.StartedTo.Value)
            {
                throw new ValidationException("StartedTo", "A data final deve ser maior ou igual a inicial.");
            }
        }

        private static void ValidatePayload(string title, string? authors, string? subject, string? summary, string? opinion, int? rating, int totalPages, int currentPage, ReadingFormat format, ReadingStatus status, DateTime? startedAt, DateTime? finishedAt)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ValidationException("Title", "O titulo da leitura e obrigatorio.");
            }

            if (title.Trim().Length > 200)
            {
                throw new ValidationException("Title", "O titulo da leitura deve ter no maximo 200 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(authors) && authors.Trim().Length > 300)
            {
                throw new ValidationException("Authors", "Os autores devem ter no maximo 300 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(subject) && subject.Trim().Length > 200)
            {
                throw new ValidationException("Subject", "O assunto deve ter no maximo 200 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(summary) && summary.Trim().Length > 4000)
            {
                throw new ValidationException("Summary", "O resumo deve ter no maximo 4000 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(opinion) && opinion.Trim().Length > 4000)
            {
                throw new ValidationException("Opinion", "A opiniao deve ter no maximo 4000 caracteres.");
            }

            if (rating.HasValue && (rating.Value < 0 || rating.Value > 5))
            {
                throw new ValidationException("Rating", "A avaliacao deve estar entre zero e cinco.");
            }

            if (totalPages <= 0)
            {
                throw new ValidationException("TotalPages", "O total de paginas deve ser maior que zero.");
            }

            if (currentPage < 0 || currentPage > totalPages)
            {
                throw new ValidationException("CurrentPage", "A pagina atual deve estar entre zero e o total de paginas.");
            }

            if (!Enum.IsDefined(format))
            {
                throw new ValidationException("Format", "Formato da leitura invalido.");
            }

            if (!Enum.IsDefined(status))
            {
                throw new ValidationException("Status", "Status da leitura invalido.");
            }

            if (startedAt.HasValue && finishedAt.HasValue && startedAt.Value > finishedAt.Value)
            {
                throw new ValidationException("FinishedAt", "A data de termino deve ser maior ou igual a data de inicio.");
            }
        }

        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            var alreadyExists = await _context.ReadingItems.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Ja existe uma leitura com o identificador informado pelo cliente.");
            }
        }

        private static void NormalizeProgressState(ReadingItem reading)
        {
            if (reading.CurrentPage <= 0)
            {
                reading.CurrentPage = 0;
                if (reading.Status == ReadingStatus.Reading)
                {
                    reading.StartedAt ??= SaoPauloDateTime.Now();
                }

                if (reading.Status == ReadingStatus.ToRead)
                {
                    reading.StartedAt = null;
                    reading.FinishedAt = null;
                }
            }

            if (reading.CurrentPage >= reading.TotalPages)
            {
                reading.CurrentPage = reading.TotalPages;
                reading.Status = ReadingStatus.Completed;
                reading.StartedAt ??= reading.FinishedAt ?? SaoPauloDateTime.Now();
                reading.FinishedAt ??= SaoPauloDateTime.Now();
                return;
            }

            if (reading.Status == ReadingStatus.Completed)
            {
                reading.CurrentPage = reading.TotalPages;
                reading.StartedAt ??= SaoPauloDateTime.Now();
                reading.FinishedAt ??= SaoPauloDateTime.Now();
            }
            else if (reading.Status == ReadingStatus.Reading)
            {
                if (reading.CurrentPage == 0)
                {
                    reading.CurrentPage = 1;
                }

                reading.StartedAt ??= SaoPauloDateTime.Now();
                reading.FinishedAt = null;
            }
            else
            {
                reading.FinishedAt = null;
            }
        }

        private async Task AwardCompletionPointsAsync(ReadingItem reading, CancellationToken cancellationToken)
        {
            var alreadyExists = await _context.UserBonusPoints.AnyAsync(x =>
                x.UserUid == reading.UserUid &&
                x.Source == BonusPointSource.ReadingCompletion &&
                x.SourceReferenceId == reading.Id, cancellationToken);

            if (alreadyExists)
            {
                return;
            }

            var rule = await _context.BonusPointRules.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Source == BonusPointSource.ReadingCompletion && x.IsActive, cancellationToken);

            if (rule is null || rule.DefaultPoints <= 0)
            {
                return;
            }

            await _context.UserBonusPoints.AddAsync(new UserBonusPoint
            {
                UserUid = reading.UserUid,
                Points = rule.DefaultPoints,
                Source = BonusPointSource.ReadingCompletion,
                SourceReferenceId = reading.Id,
                Description = $"Conclusao da leitura '{reading.Title}'."
            }, cancellationToken);
        }

        private async Task RevertCompletionPointsAsync(ReadingItem reading, CancellationToken cancellationToken)
        {
            var entries = await _context.UserBonusPoints.Where(x =>
                x.UserUid == reading.UserUid &&
                x.Source == BonusPointSource.ReadingCompletion &&
                x.SourceReferenceId == reading.Id).ToListAsync(cancellationToken);

            if (entries.Count > 0)
            {
                _context.UserBonusPoints.RemoveRange(entries);
            }
        }

        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string? NormalizeLongText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static ReadingDetailsResponse MapDetails(ReadingItem reading)
        {
            return new ReadingDetailsResponse
            {
                Id = reading.Id,
                UserUid = reading.UserUid,
                Title = reading.Title,
                Authors = reading.Authors,
                Subject = reading.Subject,
                Summary = reading.Summary,
                Opinion = reading.Opinion,
                Rating = reading.Rating,
                TotalPages = reading.TotalPages,
                CurrentPage = reading.CurrentPage,
                Genre = reading.Genre,
                Format = reading.Format,
                Status = reading.Status,
                StartedAt = reading.StartedAt,
                FinishedAt = reading.FinishedAt,
                CreatedAt = reading.CreatedAt,
                UpdatedAt = reading.UpdatedAt
            };
        }
    }
}
