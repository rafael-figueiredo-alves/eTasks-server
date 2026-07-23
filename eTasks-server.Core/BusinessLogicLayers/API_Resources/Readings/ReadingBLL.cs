using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Readings.Requests;
using eTasks_server.Models.DTOs.Readings.Responses;
using eTasks_server.Models.Entities.Readings;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Readings;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Readings
{
    /// <summary>
    /// Classe de regras de negócio do recurso de gestão de Leituras
    /// </summary>
    /// <param name="context">Contexto de dados</param>
    /// <param name="logger">Serviço de log</param>
    public class ReadingBLL(AppDbContext context, ILogger<IReadingBLL> logger) : BaseBLL<IReadingBLL>(context, logger), IReadingBLL
    {
        #region Funções principais da entidade
        /// <summary>
        /// OObtem lista das leituras
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="request">Dados/filtros</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<ReadingListItemResponse>> ListAsync(Guid userUid, ListReadingsRequest request, CancellationToken cancellationToken = default)
        {
            // Obtem e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Normaliza dados da requisição para realizar filtro dos dados
            NormalizeListRequest(request);

            // Valida filtros enviados
            ValidateListRequest(request);

            // Obtém lista completa exceto leituras removidas
            var query = _context.ReadingItems
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Se foi passado filtro por Status, filtra
            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            // Se foi passado filtro por Formato, filtra
            if (request.Format.HasValue)
            {
                query = query.Where(x => x.Format == request.Format.Value);
            }

            // Se foi passado filtro por Genero, filtra
            if (!string.IsNullOrWhiteSpace(request.Genre))
            {
                query = query.Where(x => x.Genre != null && x.Genre.Contains(request.Genre));
            }

            // Se foi passado filtro por Classificação (Ranking), filtra
            if (request.RatingFrom.HasValue)
            {
                query = query.Where(x => x.Rating.HasValue && x.Rating.Value >= request.RatingFrom.Value);
            }

            // Se foi passado filtro por Classificação (Ranking), filtra
            if (request.RatingTo.HasValue)
            {
                query = query.Where(x => x.Rating.HasValue && x.Rating.Value <= request.RatingTo.Value);
            }

            // Se foi passado filtro por Inicio da leitura, filtra
            if (request.StartedFrom.HasValue)
            {
                query = query.Where(x => x.StartedAt.HasValue && x.StartedAt.Value >= request.StartedFrom.Value);
            }

            // Se foi passado filtro por periodo da leitura, filtra
            if (request.StartedTo.HasValue)
            {
                query = query.Where(x => x.StartedAt.HasValue && x.StartedAt.Value <= request.StartedTo.Value);
            }

            // Se foi passado filtro por Termo a buscar em Autores, Assunto ou resumo, filtra
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(x =>
                    x.Title.Contains(searchTerm) ||
                    (x.Authors != null && x.Authors.Contains(searchTerm)) ||
                    (x.Subject != null && x.Subject.Contains(searchTerm)) ||
                    (x.Summary != null && x.Summary.Contains(searchTerm)));
            }

            // Retorna lista organizada por status e título
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

        /// <summary>
        /// Retorna Leitura com id informado
        /// </summary>
        /// <param name="userUid"><Uid do usuário/param>
        /// <param name="readingId">Id da leitura</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ReadingDetailsResponse> GetByIdAsync(Guid userUid, Guid readingId, CancellationToken cancellationToken = default)
        {
            // Obtém e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtém leitura do Id informado
            var reading = await _context.ReadingItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDeleted, cancellationToken);

            // Valida se ela existe
            reading = EnsureFound(reading, "Leitura não encontrada.");

            // Garante que é posse do usuário
            EnsureOwnership(reading.UserUid, userUid);

            // Mapeia entidade a retornar
            return MapDetails(reading);
        }

        /// <summary>
        /// Cria uma nova leitura
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="request">Dados a lançar</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ReadingDetailsResponse> CreateAsync(Guid userUid, CreateReadingRequest request, CancellationToken cancellationToken = default)
        {
            // Obtém e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida dados informados
            ValidatePayload(request.Title, request.Authors, request.Subject, request.Summary, request.Opinion, request.Rating, request.TotalPages, request.CurrentPage, request.Format, request.Status, request.StartedAt, request.FinishedAt);

            // Valida Id gerado pelo cliente offline
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            // Monta item de leitura
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

            // Normaliza o progresso da leitura
            NormalizeProgressState(reading);

            // Executa operações em transação
            await ExecuteInTransactionAsync(async () =>
            {
                // Salva dados
                await _context.ReadingItems.AddAsync(reading, cancellationToken);
                await SaveChangesContextAsync(cancellationToken);

                // Se leitura foi concluída
                if (reading.Status == ReadingStatus.Completed)
                {
                    // Atribui pontos ao usuário
                    await AwardCompletionPointsAsync(reading, cancellationToken);
                    await SaveChangesContextAsync(cancellationToken);
                }
            });

            // Retorna dados da leitura adicionada
            return await GetByIdAsync(userUid, reading.Id, cancellationToken);
        }

        /// <summary>
        /// Atualiza uma leitura
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="readingId">Id da leitura</param>
        /// <param name="request">Dados a atualizar</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ReadingDetailsResponse> UpdateAsync(Guid userUid, Guid readingId, UpdateReadingRequest request, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida dados a editar da leitura
            ValidatePayload(request.Title, request.Authors, request.Subject, request.Summary, request.Opinion, request.Rating, request.TotalPages, request.CurrentPage, request.Format, request.Status, request.StartedAt, request.FinishedAt);

            // Obtém leitura
            var reading = await _context.ReadingItems.FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDeleted, cancellationToken);

            // Valida que a leitura exista
            reading = EnsureFound(reading, "Leitura não encontrada.");

            // Valida que pertença ao usuário
            EnsureOwnership(reading.UserUid, userUid);

            // Guarda estado da leitura (se estava concluída)
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

            // Normaliza o progresso
            NormalizeProgressState(reading);

            // Executa transação
            await ExecuteInTransactionAsync(async () =>
            {
                // Valida se foi concluida ou se foi desmarcada conclusão
                if (!wasCompleted && reading.Status == ReadingStatus.Completed)
                {   
                    // Se concluída, atribui pontos ao usuário
                    await AwardCompletionPointsAsync(reading, cancellationToken);
                }
                else if (wasCompleted && reading.Status != ReadingStatus.Completed)
                {
                    // Se não, revoga pontos concedidos
                    await RevertCompletionPointsAsync(reading, cancellationToken);
                }

                // Salva dados
                await SaveChangesContextAsync(cancellationToken);
            });

            // Retorna leitura atualizada
            return await GetByIdAsync(userUid, reading.Id, cancellationToken);
        }

        /// <summary>
        /// Atualiza o progresso de leitura
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="readingId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<ReadingDetailsResponse> UpdateProgressAsync(Guid userUid, Guid readingId, UpdateReadingProgressRequest request, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtem leitura
            var reading = await _context.ReadingItems.FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDeleted, cancellationToken);

            // Verifica se existe
            reading = EnsureFound(reading, "Leitura não encontrada.");

            // Valida se pertence ao usuário
            EnsureOwnership(reading.UserUid, userUid);

            // Valida posição atual
            if (request.CurrentPage < 0 || request.CurrentPage > reading.TotalPages)
            {
                throw new ValidationException("CurrentPage", "A página atual deve estar entre zero e o total de páginas.");
            }

            // Verifica se houve mudança de status da leitura
            var wasCompleted = reading.Status == ReadingStatus.Completed;

            reading.CurrentPage = request.CurrentPage;
            reading.UpdatedAt = SaoPauloDateTime.Now();

            // Altera status da leitura
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

            // Executa operações de banco em transação
            await ExecuteInTransactionAsync(async () =>
            {
                // Se leitura completa
                if (!wasCompleted && reading.Status == ReadingStatus.Completed)
                {
                    // Usuário ganha pontos
                    await AwardCompletionPointsAsync(reading, cancellationToken);
                }
                else if (wasCompleted && reading.Status != ReadingStatus.Completed)
                {
                    // Senão, pontos são revogados
                    await RevertCompletionPointsAsync(reading, cancellationToken);
                }

                // Salva tudo
                await SaveChangesContextAsync(cancellationToken);
            });

            // Retorna leitura
            return await GetByIdAsync(userUid, reading.Id, cancellationToken);
        }

        /// <summary>
        /// Remove leitura
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="readingId">Id da leitura</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid userUid, Guid readingId, CancellationToken cancellationToken = default)
        {
            // Obtem e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtem leitura
            var reading = await _context.ReadingItems.FirstOrDefaultAsync(x => x.Id == readingId && !x.IsDeleted, cancellationToken);

            // Valida que exista
            reading = EnsureFound(reading, "Leitura não encontrada.");

            // Valida que pertença ao usuário
            EnsureOwnership(reading.UserUid, userUid);

            // Executa exclusão em Transação
            await ExecuteInTransactionAsync(async () =>
            {
                // Revoga pontos e marca leitura como excluída
                await RevertCompletionPointsAsync(reading, cancellationToken);
                var deletedAt = SaoPauloDateTime.Now();
                reading.IsDeleted = true;
                reading.DeletedAt = deletedAt;
                reading.UpdatedAt = deletedAt;
                await SaveChangesContextAsync(cancellationToken);
            });
        }

        /// <summary>
        /// Método para sincronizar App Cliente offline e servidor
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="request">Dados a sincronizar / Filtros</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ReadingSyncResponse> SyncAsync(Guid userUid, SyncReadingsRequest request, CancellationToken cancellationToken = default)
        {
            // Obtem e valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Rastreia operações de inserção/Edição e Exclusão
            var upsertsQuery = _context.ReadingItems.AsNoTracking().Where(x => x.UserUid == userUid && !x.IsDeleted);
            var deletedQuery = _context.ReadingItems.AsNoTracking().Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            // Valida se foi informado o filtro desde
            if (request.Since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > request.Since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > request.Since.Value);
            }

            // Gera as listas de operações
            var upserts = await upsertsQuery.OrderBy(x => x.Title).ThenBy(x => x.Id).ToListAsync(cancellationToken);
            var deleted = await deletedQuery.OrderBy(x => x.DeletedAt).ThenBy(x => x.Id)
                .Select(x => new DeletedReadingResponse { Id = x.Id, DeletedAt = x.DeletedAt!.Value })
                .ToListAsync(cancellationToken);

            // Retorna entidade de resposta
            return new ReadingSyncResponse
            {
                ServerTime = SaoPauloDateTime.Now(),
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }
        #endregion

        #region Funções auxiliares
        /// <summary>
        /// Normaliza os filtros da requisição
        /// </summary>
        /// <param name="request"></param>
        private static void NormalizeListRequest(ListReadingsRequest request)
        {
            request.Genre = string.IsNullOrWhiteSpace(request.Genre) ? null : request.Genre.Trim();
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        /// <summary>
        /// Valida filtros informados na requisição
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateListRequest(ListReadingsRequest request)
        {
            // Valida filtro por status
            if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
            {
                throw new ValidationException("Status", "Status da leitura inválido.");
            }

            // Valida filtro por formato
            if (request.Format.HasValue && !Enum.IsDefined(request.Format.Value))
            {
                throw new ValidationException("Format", "Formato da leitura inválido.");
            }
            
            // Valida filtro de Ranking
            if (request.RatingFrom.HasValue && (request.RatingFrom.Value < 0 || request.RatingFrom.Value > 5))
            {
                throw new ValidationException("RatingFrom", "A avaliação mínima deve estar entre zero e cinco.");
            }

            // Valida filtro de Ranking
            if (request.RatingTo.HasValue && (request.RatingTo.Value < 0 || request.RatingTo.Value > 5))
            {
                throw new ValidationException("RatingTo", "A avaliação máxima deve estar entre zero e cinco.");
            }

            // Valida filtro de Ranking
            if (request.RatingFrom.HasValue && request.RatingTo.HasValue && request.RatingFrom.Value > request.RatingTo.Value)
            {
                throw new ValidationException("RatingTo", "A avaliação máxima deve ser maior ou igual a mínima.");
            }

            // Valida o periodo de leitura (filtro)
            if (request.StartedFrom.HasValue && request.StartedTo.HasValue && request.StartedFrom.Value > request.StartedTo.Value)
            {
                throw new ValidationException("StartedTo", "A data final deve ser maior ou igual a inicial.");
            }
        }

        /// <summary>
        /// Valida campos da leitura
        /// </summary>
        /// <param name="title">Título</param>
        /// <param name="authors">Autores</param>
        /// <param name="subject">Assunto</param>
        /// <param name="summary">Resumo</param>
        /// <param name="opinion">Opinião</param>
        /// <param name="rating">Ranking/Classificação</param>
        /// <param name="totalPages">Páginas Totais</param>
        /// <param name="currentPage">Página atual</param>
        /// <param name="format">Formato</param>
        /// <param name="status">Status da leitura</param>
        /// <param name="startedAt">Inicio</param>
        /// <param name="finishedAt">Término</param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidatePayload(string title, string? authors, string? subject, string? summary, string? opinion, int? rating, int totalPages, int currentPage, ReadingFormat format, ReadingStatus status, DateTime? startedAt, DateTime? finishedAt)
        {
            // Valida título
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ValidationException("Title", "O título da leitura é obrigatório.");
            }

            // Valida tamanho do título
            if (title.Trim().Length > 200)
            {
                throw new ValidationException("Title", "O titulo da leitura deve ter no máximo 200 caracteres.");
            }

            // Valida campo autor
            if (!string.IsNullOrWhiteSpace(authors) && authors.Trim().Length > 300)
            {
                throw new ValidationException("Authors", "Os autores devem ter no máximo 300 caracteres.");
            }

            // Valida campo Assunto
            if (!string.IsNullOrWhiteSpace(subject) && subject.Trim().Length > 200)
            {
                throw new ValidationException("Subject", "O assunto deve ter no máximo 200 caracteres.");
            }

            // Valida campo Resumo
            if (!string.IsNullOrWhiteSpace(summary) && summary.Trim().Length > 4000)
            {
                throw new ValidationException("Summary", "O resumo deve ter no máximo 4000 caracteres.");
            }

            // Valida campo Opinião
            if (!string.IsNullOrWhiteSpace(opinion) && opinion.Trim().Length > 4000)
            {
                throw new ValidationException("Opinion", "A opinião deve ter no máximo 4000 caracteres.");
            }

            // Valida Classificação
            if (rating.HasValue && (rating.Value < 0 || rating.Value > 5))
            {
                throw new ValidationException("Rating", "A avaliação deve estar entre zero e cinco.");
            }

            // Valida qtd de páginas
            if (totalPages <= 0)
            {
                throw new ValidationException("TotalPages", "O total de páginas deve ser maior que zero.");
            }

            // Valida página atual
            if (currentPage < 0 || currentPage > totalPages)
            {
                throw new ValidationException("CurrentPage", "A página atual deve estar entre zero e o total de páginas.");
            }

            // Valida formato da publicação
            if (!Enum.IsDefined(format))
            {
                throw new ValidationException("Format", "Formato da leitura inválido.");
            }

            // Valida status da leitura
            if (!Enum.IsDefined(status))
            {
                throw new ValidationException("Status", "Status da leitura inválido.");
            }

            // Valida período da leitura
            if (startedAt.HasValue && finishedAt.HasValue && startedAt.Value > finishedAt.Value)
            {
                throw new ValidationException("FinishedAt", "A data de término deve ser maior ou igual a data de inicio.");
            }
        }

        /// <summary>
        /// Valida o Id gerado pelo cliente offline
        /// </summary>
        /// <param name="clientGeneratedId">Id gerado pelo cliente offline</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            // Valida se não está vazio
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            // Valida se já existe ID
            var alreadyExists = await _context.ReadingItems.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);

            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Já existe uma leitura com o identificador informado pelo cliente offline.");
            }
        }

        /// <summary>
        /// Normaliza status da leitura
        /// </summary>
        /// <param name="reading">Leitura</param>
        private static void NormalizeProgressState(ReadingItem reading)
        {
            // Valida se página atual é igual ou menor que zero
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

            // Valida se página atual é igual ou maior que total de páginas
            if (reading.CurrentPage >= reading.TotalPages)
            {
                reading.CurrentPage = reading.TotalPages;
                reading.Status = ReadingStatus.Completed;
                reading.StartedAt ??= reading.FinishedAt ?? SaoPauloDateTime.Now();
                reading.FinishedAt ??= SaoPauloDateTime.Now();
                return;
            }

            // Valida se leitura está concluída
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

        /// <summary>
        /// Atribui pontos ao usuário por leitura
        /// </summary>
        /// <param name="reading">Leitura</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task AwardCompletionPointsAsync(ReadingItem reading, CancellationToken cancellationToken)
        {
            // Valida se já há pontos atribuídos ao usuário pela mesma leitura
            var alreadyExists = await _context.UserBonusPoints.AnyAsync(x =>
                x.UserUid == reading.UserUid &&
                x.Source == BonusPointSource.ReadingCompletion &&
                x.SourceReferenceId == reading.Id, cancellationToken);

            // se já existir, ignora
            if (alreadyExists)
            {
                return;
            }

            // Obtém a regra de Bonus
            var rule = await _context.BonusPointRules.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Source == BonusPointSource.ReadingCompletion && x.IsActive, cancellationToken);

            // se não houver regra ou for dado 0 pontos, ignora
            if (rule is null || rule.DefaultPoints <= 0)
            {
                return;
            }

            // Adiciona os pontos
            await _context.UserBonusPoints.AddAsync(new UserBonusPoint
            {
                UserUid = reading.UserUid,
                Points = rule.DefaultPoints,
                Source = BonusPointSource.ReadingCompletion,
                SourceReferenceId = reading.Id,
                Description = $"Conclusão da leitura '{reading.Title}'."
            }, cancellationToken);
        }

        /// <summary>
        /// Rotina de revogação de pontos do usuário ganhos com leituras
        /// </summary>
        /// <param name="reading">Leitura</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task RevertCompletionPointsAsync(ReadingItem reading, CancellationToken cancellationToken)
        {
            // Obtém dados dos pontos ganhos
            var entries = await _context.UserBonusPoints.Where(x =>
                x.UserUid == reading.UserUid &&
                x.Source == BonusPointSource.ReadingCompletion &&
                x.SourceReferenceId == reading.Id).ToListAsync(cancellationToken);

            // Se encontrar, remove
            if (entries.Count > 0)
            {
                _context.UserBonusPoints.RemoveRange(entries);
            }
        }

        /// <summary>
        /// Normalizador de textos curtos
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Normalizador de textos longos
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string? NormalizeLongText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Método mapeador dos dados de resposta
        /// </summary>
        /// <param name="reading"></param>
        /// <returns></returns>
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
        #endregion
    }
}
