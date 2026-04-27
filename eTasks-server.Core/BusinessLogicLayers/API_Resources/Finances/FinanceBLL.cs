using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.DTOs.Finances.Responses;
using eTasks_server.Models.Entities.Common;
using eTasks_server.Models.Entities.Finances;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Finances
{
    public class FinanceBLL(AppDbContext context, ILogger<IFinanceBLL> logger) : BaseBLL<IFinanceBLL>(context, logger), IFinanceBLL
    {
        public async Task<List<FinanceEntryListItemResponse>> ListAsync(Guid userUid, ListFinanceEntriesRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            NormalizeListRequest(request);
            ValidateListRequest(request);

            var query = _context.FinanceEntries
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            if (request.Year.HasValue)
            {
                query = query.Where(x => x.OccursOn.Year == request.Year.Value);
            }

            if (request.Month.HasValue)
            {
                query = query.Where(x => x.OccursOn.Month == request.Month.Value);
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(x => x.OccursOn >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                query = query.Where(x => x.OccursOn <= request.DateTo.Value);
            }

            if (request.EntryType.HasValue)
            {
                query = query.Where(x => x.EntryType == request.EntryType.Value);
            }

            if (request.PaymentMethod.HasValue)
            {
                query = query.Where(x => x.PaymentMethod == request.PaymentMethod.Value);
            }

            if (request.IsPaid.HasValue)
            {
                query = query.Where(x => x.IsPaid == request.IsPaid.Value);
            }

            if (request.IsRecurring.HasValue)
            {
                query = query.Where(x => x.IsRecurring == request.IsRecurring.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query = query.Where(x => x.Category != null && x.Category.Contains(request.Category));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Title.Contains(searchTerm) ||
                    (x.Description != null && x.Description.Contains(searchTerm)) ||
                    (x.Counterparty != null && x.Counterparty.Contains(searchTerm)));
            }

            return await query
                .OrderByDescending(x => x.OccursOn)
                .ThenBy(x => x.Title)
                .Select(x => new FinanceEntryListItemResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Category = x.Category,
                    Counterparty = x.Counterparty,
                    EntryType = x.EntryType,
                    PaymentMethod = x.PaymentMethod,
                    Amount = x.Amount,
                    OccursOn = x.OccursOn,
                    IsPaid = x.IsPaid,
                    PaidAt = x.PaidAt,
                    IsRecurring = x.IsRecurring
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<FinanceEntryDetailsResponse> GetByIdAsync(Guid userUid, Guid financeEntryId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var entry = await _context.FinanceEntries
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == financeEntryId && !x.IsDeleted, cancellationToken);

            entry = EnsureFound(entry, "Lancamento financeiro nao encontrado.");
            EnsureOwnership(entry.UserUid, userUid);

            return MapDetails(entry);
        }

        public async Task<FinanceEntryDetailsResponse> CreateAsync(Guid userUid, CreateFinanceEntryRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Title, request.Description, request.Category, request.Counterparty, request.EntryType, request.PaymentMethod, request.Amount, request.OccursOn, request.IsPaid, request.PaidAt, request.IsRecurring, request.Recurrence);
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            var entry = new FinanceEntry
            {
                Id = request.ClientGeneratedId ?? Guid.CreateVersion7(),
                UserUid = userUid,
                Title = request.Title.Trim(),
                Description = NormalizeText(request.Description),
                Category = NormalizeText(request.Category),
                Counterparty = NormalizeText(request.Counterparty),
                EntryType = request.EntryType,
                PaymentMethod = request.PaymentMethod,
                Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
                OccursOn = request.OccursOn,
                IsPaid = request.IsPaid,
                PaidAt = request.IsPaid ? request.PaidAt ?? request.OccursOn : null,
                IsRecurring = request.IsRecurring,
                CreatedAt = SaoPauloDateTime.Now()
            };

            ApplyRecurrence(entry, request.Recurrence);

            await _context.FinanceEntries.AddAsync(entry, cancellationToken);
            await SaveChangesContextAsync(cancellationToken);

            return await GetByIdAsync(userUid, entry.Id, cancellationToken);
        }

        public async Task<FinanceEntryDetailsResponse> UpdateAsync(Guid userUid, Guid financeEntryId, UpdateFinanceEntryRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Title, request.Description, request.Category, request.Counterparty, request.EntryType, request.PaymentMethod, request.Amount, request.OccursOn, request.IsPaid, request.PaidAt, request.IsRecurring, request.Recurrence);

            var entry = await _context.FinanceEntries
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == financeEntryId && !x.IsDeleted, cancellationToken);
            entry = EnsureFound(entry, "Lancamento financeiro nao encontrado.");
            EnsureOwnership(entry.UserUid, userUid);

            entry.Title = request.Title.Trim();
            entry.Description = NormalizeText(request.Description);
            entry.Category = NormalizeText(request.Category);
            entry.Counterparty = NormalizeText(request.Counterparty);
            entry.EntryType = request.EntryType;
            entry.PaymentMethod = request.PaymentMethod;
            entry.Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero);
            entry.OccursOn = request.OccursOn;
            entry.IsPaid = request.IsPaid;
            entry.PaidAt = request.IsPaid ? request.PaidAt ?? request.OccursOn : null;
            entry.IsRecurring = request.IsRecurring;
            entry.UpdatedAt = SaoPauloDateTime.Now();

            ApplyRecurrence(entry, request.Recurrence);

            await SaveChangesContextAsync(cancellationToken);
            return await GetByIdAsync(userUid, entry.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid userUid, Guid financeEntryId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var entry = await _context.FinanceEntries.FirstOrDefaultAsync(x => x.Id == financeEntryId && !x.IsDeleted, cancellationToken);
            entry = EnsureFound(entry, "Lancamento financeiro nao encontrado.");
            EnsureOwnership(entry.UserUid, userUid);

            var deletedAt = SaoPauloDateTime.Now();
            entry.IsDeleted = true;
            entry.DeletedAt = deletedAt;
            entry.UpdatedAt = deletedAt;

            await SaveChangesContextAsync(cancellationToken);
        }

        public async Task<FinanceMonthSummaryResponse> GetMonthSummaryAsync(Guid userUid, int year, int month, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            if (year < 2000 || year > 9999)
            {
                throw new ValidationException("Year", "Ano invalido.");
            }

            if (month < 1 || month > 12)
            {
                throw new ValidationException("Month", "Mes invalido.");
            }

            var entries = await _context.FinanceEntries
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted && x.OccursOn.Year == year && x.OccursOn.Month == month)
                .ToListAsync(cancellationToken);

            var totalCredits = entries.Where(x => x.EntryType == FinanceEntryType.Credit).Sum(x => x.Amount);
            var totalDebits = entries.Where(x => x.EntryType == FinanceEntryType.Debit).Sum(x => x.Amount);
            var balance = totalCredits - totalDebits;
            var eligibleRuleExists = await _context.BonusPointRules.AsNoTracking()
                .AnyAsync(x => x.Source == BonusPointSource.PositiveMonthlyBalance && x.IsActive, cancellationToken);

            return new FinanceMonthSummaryResponse
            {
                Year = year,
                Month = month,
                TotalCredits = totalCredits,
                TotalDebits = totalDebits,
                Balance = balance,
                IsPositiveBalance = balance > 0,
                EligibleForBonusPoints = balance > 0 && eligibleRuleExists
            };
        }

        public async Task<FinanceEntrySyncResponse> SyncAsync(Guid userUid, SyncFinanceEntriesRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var upsertsQuery = _context.FinanceEntries.AsNoTracking().Include(x => x.Recurrence).Where(x => x.UserUid == userUid && !x.IsDeleted);
            var deletedQuery = _context.FinanceEntries.AsNoTracking().Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            if (request.Since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > request.Since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > request.Since.Value);
            }

            var upserts = await upsertsQuery.OrderByDescending(x => x.OccursOn).ThenBy(x => x.Id).ToListAsync(cancellationToken);
            var deleted = await deletedQuery.OrderBy(x => x.DeletedAt).ThenBy(x => x.Id)
                .Select(x => new DeletedFinanceEntryResponse { Id = x.Id, DeletedAt = x.DeletedAt!.Value })
                .ToListAsync(cancellationToken);

            return new FinanceEntrySyncResponse
            {
                ServerTime = SaoPauloDateTime.Now(),
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }

        private static void NormalizeListRequest(ListFinanceEntriesRequest request)
        {
            request.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        private static void ValidateListRequest(ListFinanceEntriesRequest request)
        {
            if (request.Year.HasValue && (request.Year.Value < 2000 || request.Year.Value > 9999))
            {
                throw new ValidationException("Year", "Ano invalido.");
            }

            if (request.Month.HasValue && (request.Month.Value < 1 || request.Month.Value > 12))
            {
                throw new ValidationException("Month", "Mes invalido.");
            }

            if (request.DateFrom.HasValue && request.DateTo.HasValue && request.DateFrom.Value > request.DateTo.Value)
            {
                throw new ValidationException("DateTo", "A data final deve ser maior ou igual a inicial.");
            }

            if (request.EntryType.HasValue && !Enum.IsDefined(request.EntryType.Value))
            {
                throw new ValidationException("EntryType", "Tipo de lancamento invalido.");
            }

            if (request.PaymentMethod.HasValue && !Enum.IsDefined(request.PaymentMethod.Value))
            {
                throw new ValidationException("PaymentMethod", "Forma de pagamento invalida.");
            }
        }

        private static void ValidatePayload(string title, string? description, string? category, string? counterparty, FinanceEntryType entryType, FinancePaymentMethod paymentMethod, decimal amount, DateTime occursOn, bool isPaid, DateTime? paidAt, bool isRecurring, FinanceRecurrenceRequest? recurrence)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ValidationException("Title", "O titulo do lancamento e obrigatorio.");
            }

            if (title.Trim().Length > 200)
            {
                throw new ValidationException("Title", "O titulo do lancamento deve ter no maximo 200 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 4000)
            {
                throw new ValidationException("Description", "A descricao deve ter no maximo 4000 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(category) && category.Trim().Length > 120)
            {
                throw new ValidationException("Category", "A categoria deve ter no maximo 120 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(counterparty) && counterparty.Trim().Length > 200)
            {
                throw new ValidationException("Counterparty", "A contraparte deve ter no maximo 200 caracteres.");
            }

            if (!Enum.IsDefined(entryType))
            {
                throw new ValidationException("EntryType", "Tipo de lancamento invalido.");
            }

            if (!Enum.IsDefined(paymentMethod))
            {
                throw new ValidationException("PaymentMethod", "Forma de pagamento invalida.");
            }

            if (amount <= 0)
            {
                throw new ValidationException("Amount", "O valor do lancamento deve ser maior que zero.");
            }

            if (occursOn == default)
            {
                throw new ValidationException("OccursOn", "A data de ocorrencia e obrigatoria.");
            }

            if (!isPaid && paidAt.HasValue)
            {
                throw new ValidationException("PaidAt", "Nao informe PaidAt quando o lancamento ainda nao estiver pago.");
            }

            if (isPaid && paidAt.HasValue && paidAt.Value < occursOn.AddYears(-5))
            {
                throw new ValidationException("PaidAt", "A data de pagamento informada e invalida.");
            }

            ValidateRecurrence(isRecurring, recurrence);
        }

        private static void ValidateRecurrence(bool isRecurring, FinanceRecurrenceRequest? recurrence)
        {
            if (!isRecurring)
            {
                return;
            }

            if (recurrence is null)
            {
                throw new ValidationException("Recurrence", "Informe os dados de recorrencia quando o lancamento for recorrente.");
            }

            if (!Enum.IsDefined(recurrence.RecurrenceType) || recurrence.RecurrenceType == RecurrenceType.None)
            {
                throw new ValidationException("Recurrence.RecurrenceType", "Tipo de recorrencia invalido.");
            }

            if (recurrence.RecurrenceInterval < 1)
            {
                throw new ValidationException("Recurrence.RecurrenceInterval", "O intervalo da recorrencia deve ser maior que zero.");
            }

            if (recurrence.RecurrenceType == RecurrenceType.Weekly && recurrence.WeekDays == WeekDays.None)
            {
                throw new ValidationException("Recurrence.WeekDays", "Informe ao menos um dia da semana para recorrencia semanal.");
            }

            if (recurrence.RecurrenceType == RecurrenceType.Monthly && (!recurrence.DayOfMonth.HasValue || recurrence.DayOfMonth.Value < 1 || recurrence.DayOfMonth.Value > 31))
            {
                throw new ValidationException("Recurrence.DayOfMonth", "Informe um dia do mes valido para recorrencia mensal.");
            }
        }

        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            var alreadyExists = await _context.FinanceEntries.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Ja existe um lancamento com o identificador informado pelo cliente.");
            }
        }

        private void ApplyRecurrence(FinanceEntry entry, FinanceRecurrenceRequest? recurrence)
        {
            if (!entry.IsRecurring || recurrence is null)
            {
                if (entry.Recurrence is not null)
                {
                    _context.FinanceRecurrences.Remove(entry.Recurrence);
                }

                entry.Recurrence = null;
                return;
            }

            entry.Recurrence ??= new FinanceRecurrence
            {
                FinanceEntryId = entry.Id
            };

            entry.Recurrence.RecurrenceType = recurrence.RecurrenceType;
            entry.Recurrence.Interval = recurrence.RecurrenceInterval;
            entry.Recurrence.WeekDays = recurrence.WeekDays;
            entry.Recurrence.DayOfMonth = recurrence.DayOfMonth;
            entry.Recurrence.EndsOn = recurrence.RecurrenceEndsOn;
        }

        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static FinanceEntryDetailsResponse MapDetails(FinanceEntry entry)
        {
            return new FinanceEntryDetailsResponse
            {
                Id = entry.Id,
                UserUid = entry.UserUid,
                Title = entry.Title,
                Description = entry.Description,
                Category = entry.Category,
                Counterparty = entry.Counterparty,
                EntryType = entry.EntryType,
                PaymentMethod = entry.PaymentMethod,
                Amount = entry.Amount,
                OccursOn = entry.OccursOn,
                IsPaid = entry.IsPaid,
                PaidAt = entry.PaidAt,
                IsRecurring = entry.IsRecurring,
                Recurrence = !entry.IsRecurring || entry.Recurrence is null || entry.Recurrence.RecurrenceType == RecurrenceType.None ? null : new FinanceRecurrenceResponse
                {
                    RecurrenceType = entry.Recurrence.RecurrenceType,
                    RecurrenceInterval = entry.Recurrence.Interval,
                    WeekDays = entry.Recurrence.WeekDays,
                    DayOfMonth = entry.Recurrence.DayOfMonth,
                    RecurrenceEndsOn = entry.Recurrence.EndsOn
                },
                CreatedAt = entry.CreatedAt,
                UpdatedAt = entry.UpdatedAt
            };
        }
    }
}
