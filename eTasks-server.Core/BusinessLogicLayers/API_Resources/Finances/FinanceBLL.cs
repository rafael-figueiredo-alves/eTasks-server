using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.DTOs.Finances.Responses;
using eTasks_server.Models.Entities.Finances;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Common;
using eTasks_server.Models.Enums.Finances;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Finances
{
    /// <summary>
    /// Regras de negocio para lancamentos financeiros da API.
    /// </summary>
    public class FinanceBLL(AppDbContext context, ILogger<IFinanceBLL> logger) : BaseBLL<IFinanceBLL>(context, logger), IFinanceBLL
    {
        #region Métodos principais
        /// <summary>
        /// Lista os lancamentos financeiros do usuario com filtros opcionais.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Parametros de filtro.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de lancamentos financeiros.</returns>
        public async Task<List<FinanceEntryListItemResponse>> ListAsync(Guid userUid, ListFinanceEntriesRequest request, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Normalizar parâmetros de filtro
            NormalizeListRequest(request);

            // Validar filtros / parâmetros
            ValidateListRequest(request);

            // Obtem lista de lançamentos não removidos
            var query = _context.FinanceEntries
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Valida filtro e pega títulos para o ano informado
            if (request.Year.HasValue)
            {
                query = query.Where(x => x.OccursOn.Year == request.Year.Value);
            }

            // Valida filtro e pega títulos para o mês informado
            if (request.Month.HasValue)
            {
                query = query.Where(x => x.OccursOn.Month == request.Month.Value);
            }

            // Valida filtro e pega títulos para a data inicial informada
            if (request.DateFrom.HasValue)
            {
                query = query.Where(x => x.OccursOn >= request.DateFrom.Value);
            }

            // Valida filtro e pega títulos para a data final informada
            if (request.DateTo.HasValue)
            {
                query = query.Where(x => x.OccursOn <= request.DateTo.Value);
            }

            // Valida filtro e pega títulos para o Tipo informado
            if (request.EntryType.HasValue)
            {
                query = query.Where(x => x.EntryType == request.EntryType.Value);
            }

            // Valida filtro e pega títulos para a forma de pagamento informada
            if (request.PaymentMethod.HasValue)
            {
                query = query.Where(x => x.PaymentMethod == request.PaymentMethod.Value);
            }

            // Valida filtro e pega títulos que estejam pagos ou não
            if (request.IsPaid.HasValue)
            {
                query = query.Where(x => x.IsPaid == request.IsPaid.Value);
            }

            // Valida filtro e pega títulos que sejam recorrentes
            if (request.IsRecurring.HasValue)
            {
                query = query.Where(x => x.IsRecurring == request.IsRecurring.Value);
            }

            // Valida filtro e pega títulos para a categoria informada
            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                query = query.Where(x => x.Category != null && x.Category.Contains(request.Category));
            }

            // Valida filtro e pega títulos para o termo a buscar informado
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Title.Contains(searchTerm) ||
                    (x.Description != null && x.Description.Contains(searchTerm)) ||
                    (x.Counterparty != null && x.Counterparty.Contains(searchTerm)));
            }

            // Retorna lista
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

        /// <summary>
        /// Retorna um lancamento financeiro pelo identificador.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="financeEntryId">Identificador do lancamento.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes do lancamento financeiro.</returns>
        public async Task<FinanceEntryDetailsResponse> GetByIdAsync(Guid userUid, Guid financeEntryId, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obter lançamento para id informado e que não esteja removido
            var entry = await _context.FinanceEntries
                .AsNoTracking()
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == financeEntryId && !x.IsDeleted, cancellationToken);

            // Valida se lançamento existe
            entry = EnsureFound(entry, "Lançamento financeiro não encontrado.");

            // Valida se usuário possui o lançamento
            EnsureOwnership(entry.UserUid, userUid);

            // Mapeia o lançamento a retornar
            return MapDetails(entry);
        }

        /// <summary>
        /// Cria um novo lancamento financeiro.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Dados do lancamento.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes do lancamento criado.</returns>
        public async Task<FinanceEntryDetailsResponse> CreateAsync(Guid userUid, CreateFinanceEntryRequest request, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida dados a inserir
            ValidatePayload(request.Title, request.Description, request.Category, request.Counterparty, request.EntryType, request.PaymentMethod, request.Amount, request.OccursOn, request.IsPaid, request.PaidAt, request.IsRecurring, request.Recurrence);

            // Valida o Id informado pelo cliente offline
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            // Monta a entidade para gravar
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

            // Aplica recorrência
            ApplyRecurrence(entry, request.Recurrence);

            // Adiciona e salva os dados
            await _context.FinanceEntries.AddAsync(entry, cancellationToken);
            await SaveChangesContextAsync(cancellationToken);

            // Retorna o lançamento inserido
            return await GetByIdAsync(userUid, entry.Id, cancellationToken);
        }

        /// <summary>
        /// Atualiza um lancamento financeiro existente.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="financeEntryId">Identificador do lancamento.</param>
        /// <param name="request">Novos dados do lancamento.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes do lancamento atualizado.</returns>
        public async Task<FinanceEntryDetailsResponse> UpdateAsync(Guid userUid, Guid financeEntryId, UpdateFinanceEntryRequest request, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida dados a editar
            ValidatePayload(request.Title, request.Description, request.Category, request.Counterparty, request.EntryType, request.PaymentMethod, request.Amount, request.OccursOn, request.IsPaid, request.PaidAt, request.IsRecurring, request.Recurrence);

            // Obtém registro a editar
            var entry = await _context.FinanceEntries
                .Include(x => x.Recurrence)
                .FirstOrDefaultAsync(x => x.Id == financeEntryId && !x.IsDeleted, cancellationToken);

            // Valida se registro existe
            entry = EnsureFound(entry, "Lançamento financeiro não encontrado.");

            // Valida se usuário possui o lançamento
            EnsureOwnership(entry.UserUid, userUid);

            // Grava alterações
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

            // Aplica recorrência
            ApplyRecurrence(entry, request.Recurrence);

            // Salva
            await SaveChangesContextAsync(cancellationToken);

            // Retorna lançamento
            return await GetByIdAsync(userUid, entry.Id, cancellationToken);
        }

        /// <summary>
        /// Remove logicamente um lancamento financeiro.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="financeEntryId">Identificador do lancamento.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public async Task DeleteAsync(Guid userUid, Guid financeEntryId, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtem entidade a remover
            var entry = await _context.FinanceEntries.FirstOrDefaultAsync(x => x.Id == financeEntryId && !x.IsDeleted, cancellationToken);

            // Valida se existe
            entry = EnsureFound(entry, "Lançamento financeiro não encontrado.");

            // Valida usuário possui lançamento
            EnsureOwnership(entry.UserUid, userUid);

            // Marca para remoção
            var deletedAt = SaoPauloDateTime.Now();
            entry.IsDeleted = true;
            entry.DeletedAt = deletedAt;
            entry.UpdatedAt = deletedAt;

            // Salva
            await SaveChangesContextAsync(cancellationToken);
        }

        /// <summary>
        /// Retorna o resumo mensal dos lancamentos financeiros.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="year">Ano de referencia.</param>
        /// <param name="month">Mes de referencia.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resumo financeiro do mes.</returns>
        public async Task<FinanceMonthSummaryResponse> GetMonthSummaryAsync(Guid userUid, int year, int month, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida o ano
            if (year < 2000 || year > 9999)
            {
                throw new ValidationException("Year", "Ano inválido.");
            }

            // Valida mês
            if (month < 1 || month > 12)
            {
                throw new ValidationException("Month", "Mês inválido.");
            }

            // Obtém lançamentos
            var entries = await _context.FinanceEntries
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted && x.OccursOn.Year == year && x.OccursOn.Month == month)
                .ToListAsync(cancellationToken);

            // realiza cálculo de todos os créditos, Débitos e executa balancete
            var totalCredits = entries.Where(x => x.EntryType == FinanceEntryType.Credit).Sum(x => x.Amount);
            var totalDebits = entries.Where(x => x.EntryType == FinanceEntryType.Debit).Sum(x => x.Amount);
            var balance = totalCredits - totalDebits;

            // Valida se há regra para bônus
            var eligibleRuleExists = await _context.BonusPointRules.AsNoTracking()
                .AnyAsync(x => x.Source == BonusPointSource.PositiveMonthlyBalance && x.IsActive, cancellationToken);

            // Monta resumo financeiro
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

        /// <summary>
        /// Sincroniza os lancamentos financeiros alterados desde uma data base.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Parametros de sincronizacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resposta de sincronizacao com upserts e deletados.</returns>
        public async Task<FinanceEntrySyncResponse> SyncAsync(Guid userUid, SyncFinanceEntriesRequest request, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtem registros de inserções, edições e remoções
            var upsertsQuery = _context.FinanceEntries.AsNoTracking().Include(x => x.Recurrence).Where(x => x.UserUid == userUid && !x.IsDeleted);
            var deletedQuery = _context.FinanceEntries.AsNoTracking().Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            // Valida parametro desde
            if (request.Since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > request.Since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > request.Since.Value);
            }

            // Monta listas de operações
            var upserts = await upsertsQuery.OrderByDescending(x => x.OccursOn).ThenBy(x => x.Id).ToListAsync(cancellationToken);
            var deleted = await deletedQuery.OrderBy(x => x.DeletedAt).ThenBy(x => x.Id)
                .Select(x => new DeletedFinanceEntryResponse { Id = x.Id, DeletedAt = x.DeletedAt!.Value })
                .ToListAsync(cancellationToken);

            // Retorna entidade de sincronismo
            return new FinanceEntrySyncResponse
            {
                ServerTime = SaoPauloDateTime.Now(),
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }
#endregion

        #region Métodos auxiliares
        /// <summary>
        /// Normaliza campos textuais do filtro antes da consulta.
        /// </summary>
        /// <param name="request">Parametros de listagem.</param>
        private static void NormalizeListRequest(ListFinanceEntriesRequest request)
        {
            request.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        /// <summary>
        /// Valida os filtros de listagem para evitar consultas inconsistentes.
        /// </summary>
        /// <param name="request">Parametros de listagem.</param>
        private static void ValidateListRequest(ListFinanceEntriesRequest request)
        {
            if (request.Year.HasValue && (request.Year.Value < 2000 || request.Year.Value > 9999))
            {
                throw new ValidationException("Year", "Ano inválido.");
            }

            if (request.Month.HasValue && (request.Month.Value < 1 || request.Month.Value > 12))
            {
                throw new ValidationException("Month", "Mês inválido.");
            }

            if (request.DateFrom.HasValue && request.DateTo.HasValue && request.DateFrom.Value > request.DateTo.Value)
            {
                throw new ValidationException("DateTo", "A data final deve ser maior ou igual a inicial.");
            }

            if (request.EntryType.HasValue && !Enum.IsDefined(request.EntryType.Value))
            {
                throw new ValidationException("EntryType", "Tipo de lançamento inválido.");
            }

            if (request.PaymentMethod.HasValue && !Enum.IsDefined(request.PaymentMethod.Value))
            {
                throw new ValidationException("PaymentMethod", "Forma de pagamento inválida.");
            }
        }

        /// <summary>
        /// Valida o payload de lancamento financeiro.
        /// </summary>
        /// <param name="title">Titulo do lancamento.</param>
        /// <param name="description">Descricao opcional.</param>
        /// <param name="category">Categoria opcional.</param>
        /// <param name="counterparty">Contraparte opcional.</param>
        /// <param name="entryType">Tipo de lancamento.</param>
        /// <param name="paymentMethod">Forma de pagamento.</param>
        /// <param name="amount">Valor do lancamento.</param>
        /// <param name="occursOn">Data de ocorrencia.</param>
        /// <param name="isPaid">Indica se esta pago.</param>
        /// <param name="paidAt">Data de pagamento.</param>
        /// <param name="isRecurring">Indica se e recorrente.</param>
        /// <param name="recurrence">Dados de recorrencia.</param>
        private static void ValidatePayload(string title, string? description, string? category, string? counterparty, FinanceEntryType entryType, FinancePaymentMethod paymentMethod, decimal amount, DateTime occursOn, bool isPaid, DateTime? paidAt, bool isRecurring, FinanceRecurrenceRequest? recurrence)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ValidationException("Title", "O título do lançamento é obrigatório.");
            }

            if (title.Trim().Length > 200)
            {
                throw new ValidationException("Title", "O título do lançaamento deve ter no máximo 200 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 4000)
            {
                throw new ValidationException("Description", "A descrição deve ter no máximo 4000 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(category) && category.Trim().Length > 120)
            {
                throw new ValidationException("Category", "A categoria deve ter no máximo 120 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(counterparty) && counterparty.Trim().Length > 200)
            {
                throw new ValidationException("Counterparty", "A contraparte deve ter no máximo 200 caracteres.");
            }

            if (!Enum.IsDefined(entryType))
            {
                throw new ValidationException("EntryType", "Tipo de lançamento inválido.");
            }

            if (!Enum.IsDefined(paymentMethod))
            {
                throw new ValidationException("PaymentMethod", "Forma de pagamento inválida.");
            }

            if (amount <= 0)
            {
                throw new ValidationException("Amount", "O valor do lançamento deve ser maior que zero.");
            }

            if (occursOn == default)
            {
                throw new ValidationException("OccursOn", "A data de ocorrência é obrigatória.");
            }

            if (!isPaid && paidAt.HasValue)
            {
                throw new ValidationException("PaidAt", "Não informe PaidAt quando o lançamento ainda não estiver pago.");
            }

            if (isPaid && paidAt.HasValue && paidAt.Value < occursOn.AddYears(-5))
            {
                throw new ValidationException("PaidAt", "A data de pagamento informada é inválida.");
            }

            // A recorrencia e validada em bloco separado para manter a regra isolada.
            ValidateRecurrence(isRecurring, recurrence);
        }

        /// <summary>
        /// Valida as regras especificas de recorrencia.
        /// </summary>
        /// <param name="isRecurring">Indica se a entrada e recorrente.</param>
        /// <param name="recurrence">Dados de recorrencia.</param>
        private static void ValidateRecurrence(bool isRecurring, FinanceRecurrenceRequest? recurrence)
        {
            if (!isRecurring)
            {
                return;
            }

            if (recurrence is null)
            {
                throw new ValidationException("Recurrence", "Informe os dados de recorrência quando o lançamento for recorrente.");
            }

            if (!Enum.IsDefined(recurrence.RecurrenceType) || recurrence.RecurrenceType == RecurrenceType.None)
            {
                throw new ValidationException("Recurrence.RecurrenceType", "Tipo de recorrência inválido.");
            }

            if (recurrence.RecurrenceInterval < 1)
            {
                throw new ValidationException("Recurrence.RecurrenceInterval", "O intervalo da recorrência deve ser maior que zero.");
            }

            if (recurrence.RecurrenceType == RecurrenceType.Weekly && recurrence.WeekDays == WeekDays.None)
            {
                throw new ValidationException("Recurrence.WeekDays", "Informe ao menos um dia da semana para recorrência semanal.");
            }

            if (recurrence.RecurrenceType == RecurrenceType.Monthly && (!recurrence.DayOfMonth.HasValue || recurrence.DayOfMonth.Value < 1 || recurrence.DayOfMonth.Value > 31))
            {
                throw new ValidationException("Recurrence.DayOfMonth", "Informe um dia do mês válido para recorrência mensal.");
            }
        }

        /// <summary>
        /// Garante que o identificador informado pelo cliente ainda nao exista.
        /// </summary>
        /// <param name="clientGeneratedId">Identificador opcional informado pelo cliente.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            var alreadyExists = await _context.FinanceEntries.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Já existe um lançamento com o identificador informado pelo cliente.");
            }
        }

        /// <summary>
        /// Aplica ou remove a recorrencia do lancamento conforme o payload.
        /// </summary>
        /// <param name="entry">Entidade de lancamento.</param>
        /// <param name="recurrence">Dados de recorrencia.</param>
        private void ApplyRecurrence(FinanceEntry entry, FinanceRecurrenceRequest? recurrence)
        {
            // Verifica se não é mais recorrente e apaga recorrência antiga
            if (!entry.IsRecurring || recurrence is null)
            {
                if (entry.Recurrence is not null)
                {
                    // Remove a recorrencia antiga quando a entrada deixa de ser recorrente.
                    _context.FinanceRecurrences.Remove(entry.Recurrence);
                }

                entry.Recurrence = null;
                return;
            }

            // Verifica se existe recorrência, se não existir, cria com id fornecido
            entry.Recurrence ??= new FinanceRecurrence
            {
                FinanceEntryId = entry.Id
            };

            // Configura recorrência
            entry.Recurrence.RecurrenceType = recurrence.RecurrenceType;
            entry.Recurrence.Interval = recurrence.RecurrenceInterval;
            entry.Recurrence.WeekDays = recurrence.WeekDays;
            entry.Recurrence.DayOfMonth = recurrence.DayOfMonth;
            entry.Recurrence.EndsOn = recurrence.RecurrenceEndsOn;
        }

        /// <summary>
        /// Normaliza um texto opcional para null quando vazio.
        /// </summary>
        /// <param name="value">Texto original.</param>
        /// <returns>Texto normalizado ou null.</returns>
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Mapeia a entidade de lancamento para a resposta detalhada.
        /// </summary>
        /// <param name="entry">Entidade carregada do banco.</param>
        /// <returns>Resposta com todos os detalhes do lancamento.</returns>
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
        #endregion
    }
}
