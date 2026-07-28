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
    /// <summary>
    /// Regras de negocio para anotacoes da API.
    /// </summary>
    public class NoteBLL(AppDbContext context, ILogger<INoteBLL> logger) : BaseBLL<INoteBLL>(context, logger), INoteBLL
    {
        #region Funções principais
        /// <summary>
        /// Lista as anotacoes do usuario com filtros opcionais.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Parametros de filtro.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Lista de anotacoes.</returns>
        public async Task<List<NoteListItemResponse>> ListAsync(Guid userUid, ListNotesRequest request, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Normalizar parâmetros recebidos
            NormalizeListRequest(request);

            // Validar filtros
            ValidateListRequest(request);

            // Obter todas as anotações exceto as removidas
            var query = _context.Notes
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Valida se é para retornar apenas para um termo buscado
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();
                query = query.Where(x => x.Subject.Contains(searchTerm) || x.Content.Contains(searchTerm));
            }

            // Busca por data de criação
            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
            }

            // Busca por data de criação
            if (request.CreatedTo.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= request.CreatedTo.Value);
            }

            // Busca por data de edição
            if (request.UpdatedFrom.HasValue)
            {
                query = query.Where(x => x.UpdatedAt.HasValue && x.UpdatedAt.Value >= request.UpdatedFrom.Value);
            }

            // Busca por data de edição
            if (request.UpdatedTo.HasValue)
            {
                query = query.Where(x => x.UpdatedAt.HasValue && x.UpdatedAt.Value <= request.UpdatedTo.Value);
            }

            // Retorna anotações em lista ordenados por data de edição e/ou criação
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

        /// <summary>
        /// Retorna uma anotacao pelo identificador.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="noteId">Identificador da anotacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes da anotacao.</returns>
        public async Task<NoteDetailsResponse> GetByIdAsync(Guid userUid, Guid noteId, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obter anotação pelo id informado
            var note = await _context.Notes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == noteId && !x.IsDeleted, cancellationToken);

            // Valida se anotação existe
            note = EnsureFound(note, "Anotação não encontrada.");

            // Valida posse da anotação pelo usuário informado
            EnsureOwnership(note.UserUid, userUid);

            // Retorna entidade detalhada mapeada
            return MapDetails(note);
        }

        /// <summary>
        /// Cria uma nova anotacao.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Dados da anotacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes da anotacao criada.</returns>
        public async Task<NoteDetailsResponse> CreateAsync(Guid userUid, CreateNoteRequest request, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Validar dados para criar anotação
            ValidatePayload(request.Subject, request.Content);

            // Valida ID gerado pelo cliente offline
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            // Gera a entidade a ser gravada
            var note = new NoteItem
            {
                Id = request.ClientGeneratedId ?? Guid.CreateVersion7(),
                UserUid = userUid,
                Subject = request.Subject.Trim(),
                Content = request.Content.Trim(),
                CreatedAt = SaoPauloDateTime.Now(),
                UpdatedAt = null
            };

            // Adiciona ao contexto
            await _context.Notes.AddAsync(note, cancellationToken);

            // Salva
            await SaveChangesContextAsync(cancellationToken);

            // Retorna dados
            return await GetByIdAsync(userUid, note.Id, cancellationToken);
        }

        /// <summary>
        /// Atualiza uma anotacao existente.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="noteId">Identificador da anotacao.</param>
        /// <param name="request">Novos dados da anotacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Detalhes da anotacao atualizada.</returns>
        public async Task<NoteDetailsResponse> UpdateAsync(Guid userUid, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default)
        {
            // Obter usuário e validar
            await GetAndValidateActiveUserAsync(userUid);

            // Validar dados a editar
            ValidatePayload(request.Subject, request.Content);

            // Obter anotação a editar
            var note = await _context.Notes
                .FirstOrDefaultAsync(x => x.Id == noteId && !x.IsDeleted, cancellationToken);

            // Validar que anotação existe
            note = EnsureFound(note, "Anotação não encontrada.");

            // Garante que usuário possui acesso a notificação
            EnsureOwnership(note.UserUid, userUid);

            // Edita informações 
            note.Subject = request.Subject.Trim();
            note.Content = request.Content.Trim();
            note.UpdatedAt = SaoPauloDateTime.Now();

            // Salva
            await SaveChangesContextAsync(cancellationToken);

            // Retorna dados
            return await GetByIdAsync(userUid, note.Id, cancellationToken);
        }

        /// <summary>
        /// Remove logicamente uma anotacao.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="noteId">Identificador da anotacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        public async Task DeleteAsync(Guid userUid, Guid noteId, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obter anotação com id informado
            var note = await _context.Notes
                .FirstOrDefaultAsync(x => x.Id == noteId && !x.IsDeleted, cancellationToken);

            // Valida que anotação existe
            note = EnsureFound(note, "Anotação não encontrada.");

            // Garante que usuário possui permissão da anotação
            EnsureOwnership(note.UserUid, userUid);

            // Marca anotação como excluída
            var deletedAt = SaoPauloDateTime.Now();
            note.IsDeleted = true;
            note.DeletedAt = deletedAt;
            note.UpdatedAt = deletedAt;

            // Salva
            await SaveChangesContextAsync(cancellationToken);
        }

        /// <summary>
        /// Sincroniza anotacoes alteradas desde uma data base.
        /// </summary>
        /// <param name="userUid">Identificador do usuario.</param>
        /// <param name="request">Parametros de sincronizacao.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resposta de sincronizacao com upserts e deletados.</returns>
        public async Task<NoteSyncResponse> SyncAsync(Guid userUid, SyncNotesRequest request, CancellationToken cancellationToken = default)
        {
            // Obter e validar usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obter data desde quando obter dados a sincronizar
            var since = request.Since;

            // Obtem data / hora do servidor
            var serverTime = SaoPauloDateTime.Now();

            // Obtem anotações que não foram marcadas como excluídas
            var upsertsQuery = _context.Notes
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Obtem as que foram removidas
            var deletedQuery = _context.Notes
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            // Pega a data de inicio
            if (since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > since.Value);
            }

            // Gera lista de inserções/edições
            var upserts = await upsertsQuery
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

            // Gera lista de remoções
            var deleted = await deletedQuery
                .OrderBy(x => x.DeletedAt)
                .ThenBy(x => x.Id)
                .Select(x => new DeletedNoteResponse
                {
                    Id = x.Id,
                    DeletedAt = x.DeletedAt!.Value
                })
                .ToListAsync(cancellationToken);

            // Gera resposta com operações a sincronizar
            return new NoteSyncResponse
            {
                ServerTime = serverTime,
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }
        #endregion

        #region Funções Auxiliares
        /// <summary>
        /// Normaliza os filtros textuais da listagem.
        /// </summary>
        /// <param name="request">Parametros de listagem.</param>
        private static void NormalizeListRequest(ListNotesRequest request)
        {
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        /// <summary>
        /// Valida os filtros de listagem de anotacoes.
        /// </summary>
        /// <param name="request">Parametros de listagem.</param>
        private static void ValidateListRequest(ListNotesRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchTerm) && request.SearchTerm.Trim().Length > 200)
            {
                throw new ValidationException("SearchTerm", "O termo de pesquisa deve ter no máximo 200 caracteres.");
            }

            // Cada intervalo e validado separadamente para manter o erro apontando o campo certo.
            ValidateDateRange(request.CreatedFrom, request.CreatedTo, "CreatedTo");
            ValidateDateRange(request.UpdatedFrom, request.UpdatedTo, "UpdatedTo");
        }

        /// <summary>
        /// Valida se o intervalo de datas esta em ordem crescente.
        /// </summary>
        /// <param name="from">Data inicial.</param>
        /// <param name="to">Data final.</param>
        /// <param name="fieldName">Nome do campo de destino do erro.</param>
        private static void ValidateDateRange(DateTime? from, DateTime? to, string fieldName)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                throw new ValidationException(fieldName, "A data final deve ser maior ou igual a data inicial.");
            }
        }

        /// <summary>
        /// Valida o payload de criacao ou atualizacao de anotacao.
        /// </summary>
        /// <param name="subject">Assunto da anotacao.</param>
        /// <param name="content">Conteudo da anotacao.</param>
        private static void ValidatePayload(string subject, string content)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ValidationException("Subject", "O assunto da anotação é obrigatório.");
            }

            if (subject.Trim().Length > 200)
            {
                throw new ValidationException("Subject", "O assunto da anotação deve ter no máximo 200 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ValidationException("Content", "O conteúdo da anotação é obrigatório.");
            }

            if (content.Trim().Length > 20000)
            {
                throw new ValidationException("Content", "O conteúdo da anotação deve ter no máximo 20000 caracteres.");
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

            var alreadyExists = await _context.Notes.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Já existe uma anotação com o identificador informado pelo cliente.");
            }
        }

        /// <summary>
        /// Gera uma previsualizacao curta do conteudo da anotacao.
        /// </summary>
        /// <param name="content">Conteudo original.</param>
        /// <returns>Trecho reduzido do conteudo.</returns>
        private static string BuildPreview(string content)
        {
            // Remove quebras de linha antes de cortar o texto.
            var normalized = string.Join(' ', content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            if (normalized.Length <= 180)
            {
                return normalized;
            }

            return $"{normalized[..177]}...";
        }

        /// <summary>
        /// Mapeia a entidade de anotacao para a resposta detalhada.
        /// </summary>
        /// <param name="note">Entidade carregada do banco.</param>
        /// <returns>Resposta de detalhes da anotacao.</returns>
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
        #endregion
    }
}
