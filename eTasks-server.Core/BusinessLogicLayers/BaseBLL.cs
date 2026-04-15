using eTasks_server.Core.Data;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace eTasks_server.Core.BusinessLogicLayers
{
    public abstract class BaseBLL<TInterface> where TInterface : class
    {
        protected readonly AppDbContext _context;
        protected readonly ILogger<TInterface> _logger;

        protected BaseBLL(AppDbContext context, ILogger<TInterface> logger)
        {
            _context = context;
            _logger = logger;
        }

        protected async Task<int> SaveChangesContextAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro de integridade do banco ao tentar salvar.");
                throw new ApiException(HttpStatusCode.InternalServerError, "Erro interno ao atualizar os dados. Tente novamente mais tarde.");
            }
        }

        protected T EnsureFound<T>(T? entity, string errorMessage = "Recurso não encontrado.")
        {
            if (entity == null)
            {
                _logger.LogWarning("Falha ao localizar recurso do tipo {Type}. Erro reportado: {Msg}", typeof(T).Name, errorMessage);
                throw new ApiException(HttpStatusCode.NotFound, errorMessage);
            }
            return entity;
        }

        /// <summary>
        /// Lança <see cref="ValidationException"/> se o campo informado já existe no banco,
        /// evitando duplicação de registros únicos.
        /// </summary>
        /// <param name="alreadyExists">Resultado da verificação de existência (true = duplicado).</param>
        /// <param name="fieldName">Nome do campo sendo validado (usado na mensagem e no log).</param>
        /// <param name="errorMessage">Mensagem exibida ao usuário. Se nulo, usa mensagem padrão.</param>
        protected void EnsureUnique(bool alreadyExists, string fieldName, string? errorMessage = null)
        {
            if (alreadyExists)
            {
                var msg = errorMessage ?? $"Já existe um registro com esse valor para o campo '{fieldName}'.";
                _logger.LogWarning("Tentativa de criação com {Field} duplicado.", fieldName);
                throw new ValidationException(fieldName, msg);
            }
        }

        protected void EnsureOwnership(Guid entityUserUid, Guid currentUserUid)
        {
            if (entityUserUid != currentUserUid)
            {
                _logger.LogWarning("Tentativa de acesso não autorizado: Usuário {CurrentUser} tentou acessar recurso pertencente a {OwnerUser}", currentUserUid, entityUserUid);
                throw new ApiException(HttpStatusCode.Forbidden, "Acesso negado. O recurso não pertence a você.");
            }
        }

        protected async Task<User> GetAndValidateActiveUserAsync(Guid uid, Func<IQueryable<User>, IQueryable<User>>? includeFunc = null)
        {
            var query = _context.Users.AsQueryable();

            if (includeFunc != null)
            {
                query = includeFunc(query);
            }

            var user = await query.FirstOrDefaultAsync(u => u.Uid == uid);

            if (user == null)
            {
                _logger.LogWarning("Falha ao carregar usuário {Uid}. Usuário não encontrado no banco.", uid);
                throw new ApiException(HttpStatusCode.NotFound, "Usuário não encontrado.");
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Usuário {Uid} tentou ação mas a conta está deletada lógica.", uid);
                throw new ApiException(HttpStatusCode.Forbidden, "Sua conta foi removida e não pode mais ser utilizada.");
            }

            if (user.IsBlocked)
            {
                _logger.LogWarning("Usuário {Uid} tentou ação mas a conta está bloqueada.", uid);
                throw new ApiException(HttpStatusCode.Forbidden, "Sua conta foi suspensa temporariamente. Entre em contato com o suporte.");
            }

            return user;
        }

        protected async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await action();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Rollback disparado devido a um erro na transação.");
                    throw;
                }
            });
        }
    }
}
