using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Core.Data;
using eTasks_server.Models.DTOs.Shopping.Requests;
using eTasks_server.Models.DTOs.Shopping.Responses;
using eTasks_server.Models.Entities.Shopping;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Shopping;
using eTasks_server.Models.Exceptions;
using eTasks_server.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eTasks_server.Core.BusinessLogicLayers.API_Resources.Shopping
{
    /// <summary>
    /// Classe de regras de negócio do recurso de Listas de Compras
    /// </summary>
    /// <param name="context">Contextio de dados</param>
    /// <param name="logger">Serviço de Log</param>
    public class ShoppingListBLL(AppDbContext context, ILogger<IShoppingListBLL> logger) : BaseBLL<IShoppingListBLL>(context, logger), IShoppingListBLL
    {
        #region Funções principais das listas de compras
        /// <summary>
        /// Obter lista das listas de compras disponíveis para filtros informados
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="request">Parâmetros</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<ShoppingListListItemResponse>> ListAsync(Guid userUid, ListShoppingListsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Normaliza parâmetros / filtros
            NormalizeListRequest(request);

            // Valida parâmetros / filtros
            ValidateListRequest(request);

            // Obtem listas que não se encontram marcadas como excluídas
            var query = _context.ShoppingLists
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Valida se foi aplicado filtro de apenas compras finalizadas
            if (request.IsFinalized.HasValue)
            {
                query = query.Where(x => x.IsFinalized == request.IsFinalized.Value);
            }

            // Valida se foi aplicado filtro por tipo de compra
            if (request.Type.HasValue)
            {
                query = query.Where(x => x.Type == request.Type.Value);
            }

            // Valida se foi aplicado filtro por lugar
            if (!string.IsNullOrWhiteSpace(request.Place))
            {
                query = query.Where(x => x.Place != null && x.Place.Contains(request.Place));
            }
            // Valida se foi aplicado filtro por termo a buscar

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(x => x.Name.Contains(request.SearchTerm));
            }

            // Retorna lista ordenada por finalizada ou não e nome
            return await query
                .OrderBy(x => x.IsFinalized)
                .ThenBy(x => x.Name)
                .Select(x => new ShoppingListListItemResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Place = x.Place,
                    Type = x.Type,
                    TotalItems = x.TotalItems,
                    TotalAmount = x.TotalAmount,
                    IsFinalized = x.IsFinalized,
                    CompletedItems = x.Items.Count(item => item.IsCompleted),
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtem lista de compras pelo seu ID
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="shoppingListId">Id da lista de compras</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ShoppingListDetailsResponse> GetByIdAsync(Guid userUid, Guid shoppingListId, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtem lista de compras para o id informado que não esteja marcada como excluída
            var list = await _context.ShoppingLists
                .AsNoTracking()
                .Include(x => x.Items.OrderBy(item => item.CreatedAt))
                .FirstOrDefaultAsync(x => x.Id == shoppingListId && !x.IsDeleted, cancellationToken);

            // Valida se foi possível encontrar a lista
            list = EnsureFound(list, "Lista de compras não encontrada.");

            // Valida se a lista pertence ao usuário
            EnsureOwnership(list.UserUid, userUid);

            // Mapeia entidade da resposta
            return MapDetails(list);
        }

        /// <summary>
        /// Cria uma nova lista de compras no banco de dados
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="request">Dados para criar</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ShoppingListDetailsResponse> CreateAsync(Guid userUid, CreateShoppingListRequest request, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida dados a usar para criar lista de compras
            ValidatePayload(request.Name, request.Place, request.Type, request.Items);

            // Valida o Id gerado pelo cliente offline
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

            // Gera a lista de compras
            var list = new ShoppingList
            {
                Id = request.ClientGeneratedId ?? Guid.CreateVersion7(),
                UserUid = userUid,
                Name = request.Name.Trim(),
                Place = NormalizeText(request.Place),
                Type = request.Type,
                IsFinalized = request.IsFinalized,
                CreatedAt = SaoPauloDateTime.Now()
            };

            // Preenche a lista com seus respectivos itens
            foreach (var itemRequest in request.Items)
            {
                list.Items.Add(MapNewItem(itemRequest));
            }

            // Recalcula o total
            RecalculateTotals(list);

            // Executã transição para gravar, em caso de 1 falha, não grava nada
            await ExecuteInTransactionAsync(async () =>
            {
                // Anexa nova entidade para gravação e salva no banco
                await _context.ShoppingLists.AddAsync(list, cancellationToken);
                await SaveChangesContextAsync(cancellationToken);

                // Se lista marcada como finalizada
                if (list.IsFinalized)
                {
                    // Atribui pontos ao usuário e salva dados
                    await AwardCompletionPointsAsync(list, cancellationToken);
                    await SaveChangesContextAsync(cancellationToken);
                }
            });

            // Retorna lista criada
            return await GetByIdAsync(userUid, list.Id, cancellationToken);
        }

        /// <summary>
        /// Atualiza lista de compras
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="shoppingListId">Id da lista</param>
        /// <param name="request">dados</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ShoppingListDetailsResponse> UpdateAsync(Guid userUid, Guid shoppingListId, UpdateShoppingListRequest request, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Valida dados fornecidos para atualização
            ValidatePayload(request.Name, request.Place, request.Type, request.Items.Select(x => new CreateShoppingListItemRequest
            {
                ClientGeneratedId = x.Id,
                Description = x.Description,
                Unit = x.Unit,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                IsCompleted = x.IsCompleted
            }).ToList());

            // Obtem a lista de compras
            var list = await _context.ShoppingLists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == shoppingListId && !x.IsDeleted, cancellationToken);

            // Valida se a lista existe mesmo
            list = EnsureFound(list, "Lista de compras não encontrada.");

            // Garante que lista pertence ao usuário
            EnsureOwnership(list.UserUid, userUid);

            // Guarda status anterior se estava ou não finalizada
            var wasFinalized = list.IsFinalized;

            // Salva novos dados
            list.Name = request.Name.Trim();
            list.Place = NormalizeText(request.Place);
            list.Type = request.Type;
            list.IsFinalized = request.IsFinalized;
            list.UpdatedAt = SaoPauloDateTime.Now();

            // Aplica diferenças / edições dos itens
            ApplyItemDiff(list, request.Items);

            // Recalcula totais
            RecalculateTotals(list);

            // Executa operações de gravação de dados
            await ExecuteInTransactionAsync(async () =>
            {
                // Se foi finalizada
                if (!wasFinalized && list.IsFinalized)
                {
                    // Atribui pontos
                    await AwardCompletionPointsAsync(list, cancellationToken);
                }
                else if (wasFinalized && !list.IsFinalized)
                {
                    // Se não remove pontos
                    await RevertCompletionPointsAsync(list, cancellationToken);
                }

                // Salva
                await SaveChangesContextAsync(cancellationToken);
            });

            // Retorna lista
            return await GetByIdAsync(userUid, list.Id, cancellationToken);
        }

        /// <summary>
        /// Marca lista como removida/excluída
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="shoppingListId">Id da lista</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid userUid, Guid shoppingListId, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Obtem lista de compras
            var list = await _context.ShoppingLists.FirstOrDefaultAsync(x => x.Id == shoppingListId && !x.IsDeleted, cancellationToken);

            // Valida que a lista existe
            list = EnsureFound(list, "Lista de compras não encontrada.");

            // Garante que a lista pertença ao usuário
            EnsureOwnership(list.UserUid, userUid);

            // Executa remoção em transição para se algo der errado, resetar operação (Rollback)
            await ExecuteInTransactionAsync(async () =>
            {
                // Remover pontos
                await RevertCompletionPointsAsync(list, cancellationToken);

                // Marcar como removido
                var deletedAt = SaoPauloDateTime.Now();
                list.IsDeleted = true;
                list.DeletedAt = deletedAt;
                list.UpdatedAt = deletedAt;

                // Salva
                await SaveChangesContextAsync(cancellationToken);
            });
        }

        /// <summary>
        /// Sincroniza dados
        /// </summary>
        /// <param name="userUid">Uid do usuário</param>
        /// <param name="request">Dados</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ShoppingListSyncResponse> SyncAsync(Guid userUid, SyncShoppingListsRequest request, CancellationToken cancellationToken = default)
        {
            // Valida usuário
            await GetAndValidateActiveUserAsync(userUid);

            // Gera dados de inserções e / ou edições
            var upsertsQuery = _context.ShoppingLists
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            // Obtem dados de listas removidas
            var deletedQuery = _context.ShoppingLists
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            // Valida se tem data inicial de obtenção das listas
            if (request.Since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > request.Since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > request.Since.Value);
            }

            // Obtem dados 
            var upserts = await upsertsQuery.OrderBy(x => x.Name).ThenBy(x => x.Id).ToListAsync(cancellationToken);
            var deleted = await deletedQuery.OrderBy(x => x.DeletedAt).ThenBy(x => x.Id)
                .Select(x => new DeletedShoppingListResponse { Id = x.Id, DeletedAt = x.DeletedAt!.Value })
                .ToListAsync(cancellationToken);

            // Retorna dados 
            return new ShoppingListSyncResponse
            {
                ServerTime = SaoPauloDateTime.Now(),
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }
        #endregion

        #region Funções auxiliares
        /// <summary>
        /// Normaliza dados da requisição
        /// </summary>
        /// <param name="request"></param>
        private static void NormalizeListRequest(ListShoppingListsRequest request)
        {
            request.Place = string.IsNullOrWhiteSpace(request.Place) ? null : request.Place.Trim();
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        /// <summary>
        /// Valida parametros da requisição
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateListRequest(ListShoppingListsRequest request)
        {
            if (request.Type.HasValue && !Enum.IsDefined(request.Type.Value))
            {
                throw new ValidationException("Type", "Tipo da lista de compras inválido.");
            }
        }

        /// <summary>
        /// Valida dados de inserção / edição
        /// </summary>
        /// <param name="name">Nome da lista</param>
        /// <param name="place">Lugar</param>
        /// <param name="type">Tipo</param>
        /// <param name="items">Itens</param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidatePayload(string name, string? place, ShoppingListType type, IEnumerable<CreateShoppingListItemRequest> items)
        {
            // Valida nome da lista não está em branco
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ValidationException("Name", "O nome da lista é obrigatório.");
            }

            // Valida se nome respeita tamanho máximo
            if (name.Trim().Length > 200)
            {
                throw new ValidationException("Name", "O nome da lista deve ter no máximo 200 caracteres.");
            }

            // Valida nome do lugar
            if (!string.IsNullOrWhiteSpace(place) && place.Trim().Length > 200)
            {
                throw new ValidationException("Place", "O local deve ter no máximo 200 caracteres.");
            }

            // Valida tipo da lista
            if (!Enum.IsDefined(type))
            {
                throw new ValidationException("Type", "Tipo da lista de compras inválido.");
            }

            // Gera tabela de ids virtual
            var ids = new HashSet<Guid>();

            // Valida e trata lista de itens
            foreach (var item in items)
            {
                // Valida dados dos itens
                ValidateItem(item.Description, item.Unit, item.Quantity, item.UnitPrice);

                // Valida Id gerado pelo cliente offline
                if (item.ClientGeneratedId.HasValue && !ids.Add(item.ClientGeneratedId.Value))
                {
                    throw new ValidationException("Items", "Não é permitido repetir o identificador de item na mesma lista.");
                }
            }
        }

        /// <summary>
        /// Valida item da lista
        /// </summary>
        /// <param name="description">Descrição do item</param>
        /// <param name="unit">Unidade de compra</param>
        /// <param name="quantity">Quantidade</param>
        /// <param name="unitPrice">Preço unitário</param>
        /// <exception cref="ValidationException"></exception>
        private static void ValidateItem(string description, ShoppingItemUnit unit, decimal quantity, decimal unitPrice)
        {
            // Valida se descrição não está vazio
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ValidationException("Description", "A descrição do item é obrigatória.");
            }

            // Valida tamanho máximo permitido da descrição
            if (description.Trim().Length > 200)
            {
                throw new ValidationException("Description", "A descrição do item deve ter no máximo 200 caracteres.");
            }

            // Valida a unidade
            if (!Enum.IsDefined(unit))
            {
                throw new ValidationException("Unit", "Unidade do item inválida.");
            }

            // Valida se quantidade é menor ou igual a zero, que não é permitida
            if (quantity <= 0)
            {
                throw new ValidationException("Quantity", "A quantidade do item deve ser maior que zero.");
            }

            // Valida se preço unitário é válido (maior que zero)
            if (unitPrice < 0)
            {
                throw new ValidationException("UnitPrice", "O preço unitário não pode ser negativo.");
            }
        }

        /// <summary>
        /// VAlida Id gerado pelo cliente offline
        /// </summary>
        /// <param name="clientGeneratedId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            // Valida se foi informado Id
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            // Valida se já existe Id na base de dados
            var alreadyExists = await _context.ShoppingLists.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);

            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Já existe uma lista com o identificador informado pelo cliente offline.");
            }
        }

        /// <summary>
        /// Mapeia novo item da lista
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static ShoppingListItem MapNewItem(CreateShoppingListItemRequest request)
        {
            var quantity = decimal.Round(request.Quantity, 2, MidpointRounding.AwayFromZero);
            var unitPrice = decimal.Round(request.UnitPrice, 2, MidpointRounding.AwayFromZero);

            return new ShoppingListItem
            {
                Id = request.ClientGeneratedId ?? Guid.CreateVersion7(),
                Description = request.Description.Trim(),
                Unit = request.Unit,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalAmount = decimal.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero),
                IsCompleted = request.IsCompleted,
                CreatedAt = SaoPauloDateTime.Now()
            };
        }

        /// <summary>
        /// Aplica diferenças de itens editados
        /// </summary>
        /// <param name="list">Lista</param>
        /// <param name="items">Itens</param>
        /// <exception cref="ValidationException"></exception>
        private static void ApplyItemDiff(ShoppingList list, ICollection<UpdateShoppingListItemRequest> items)
        {
            var existingById = list.Items.ToDictionary(x => x.Id);
            var requestedIds = new HashSet<Guid>();

            foreach (var itemRequest in items)
            {
                ValidateItem(itemRequest.Description, itemRequest.Unit, itemRequest.Quantity, itemRequest.UnitPrice);

                if (!requestedIds.Add(itemRequest.Id))
                {
                    throw new ValidationException("Items", "Não é permitido repetir o item dentro da mesma lista.");
                }

                var quantity = decimal.Round(itemRequest.Quantity, 2, MidpointRounding.AwayFromZero);
                var unitPrice = decimal.Round(itemRequest.UnitPrice, 2, MidpointRounding.AwayFromZero);

                if (existingById.TryGetValue(itemRequest.Id, out var existingItem))
                {
                    existingItem.Description = itemRequest.Description.Trim();
                    existingItem.Unit = itemRequest.Unit;
                    existingItem.Quantity = quantity;
                    existingItem.UnitPrice = unitPrice;
                    existingItem.TotalAmount = decimal.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);
                    existingItem.IsCompleted = itemRequest.IsCompleted;
                }
                else
                {
                    list.Items.Add(new ShoppingListItem
                    {
                        Id = itemRequest.Id == Guid.Empty ? Guid.CreateVersion7() : itemRequest.Id,
                        Description = itemRequest.Description.Trim(),
                        Unit = itemRequest.Unit,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        TotalAmount = decimal.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero),
                        IsCompleted = itemRequest.IsCompleted,
                        CreatedAt = SaoPauloDateTime.Now()
                    });
                }
            }

            var itemsToRemove = list.Items.Where(x => !requestedIds.Contains(x.Id)).ToList();
            if (itemsToRemove.Count > 0)
            {
                _ = itemsToRemove;
            }

            foreach (var item in itemsToRemove)
            {
                list.Items.Remove(item);
            }
        }

        /// <summary>
        /// Recalcula totais 
        /// </summary>
        /// <param name="list"></param>
        private static void RecalculateTotals(ShoppingList list)
        {
            list.TotalItems = list.Items.Count;
            list.TotalAmount = decimal.Round(list.Items.Sum(x => x.TotalAmount), 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Atribuir pontos por concluir uma lista de compras
        /// </summary>
        /// <param name="list"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task AwardCompletionPointsAsync(ShoppingList list, CancellationToken cancellationToken)
        {
            // Valida se existe pontos lançados
            var alreadyExists = await _context.UserBonusPoints.AnyAsync(x =>
                x.UserUid == list.UserUid &&
                x.Source == BonusPointSource.ShoppingListCompletion &&
                x.SourceReferenceId == list.Id, cancellationToken);

            if (alreadyExists)
            {
                return;
            }

            // Obtem regra a usar
            var rule = await _context.BonusPointRules.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Source == BonusPointSource.ShoppingListCompletion && x.IsActive, cancellationToken);

            // Se não houver regra ou a pontuação for igual ou menor que zero, ignora
            if (rule is null || rule.DefaultPoints <= 0)
            {
                return;
            }

            // Atribui pontos
            await _context.UserBonusPoints.AddAsync(new UserBonusPoint
            {
                UserUid = list.UserUid,
                Points = rule.DefaultPoints,
                Source = BonusPointSource.ShoppingListCompletion,
                SourceReferenceId = list.Id,
                Description = $"Finalização da lista de compras '{list.Name}'."
            }, cancellationToken);
        }

        /// <summary>
        /// Revogar pontos ganhos
        /// </summary>
        /// <param name="list"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task RevertCompletionPointsAsync(ShoppingList list, CancellationToken cancellationToken)
        {
            // Busca os pontos obtidos
            var entries = await _context.UserBonusPoints.Where(x =>
                x.UserUid == list.UserUid &&
                x.Source == BonusPointSource.ShoppingListCompletion &&
                x.SourceReferenceId == list.Id).ToListAsync(cancellationToken);

            // Se encontrar, remove
            if (entries.Count > 0)
            {
                _context.UserBonusPoints.RemoveRange(entries);
            }
        }

        /// <summary>
        /// Método para normalizar texto
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Mapeia detalhes da lista de compras
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static ShoppingListDetailsResponse MapDetails(ShoppingList list)
        {
            return new ShoppingListDetailsResponse
            {
                Id = list.Id,
                UserUid = list.UserUid,
                Name = list.Name,
                Place = list.Place,
                Type = list.Type,
                TotalItems = list.TotalItems,
                TotalAmount = list.TotalAmount,
                IsFinalized = list.IsFinalized,
                CreatedAt = list.CreatedAt,
                UpdatedAt = list.UpdatedAt,
                Items = list.Items
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => new ShoppingListItemResponse
                    {
                        Id = x.Id,
                        Description = x.Description,
                        Unit = x.Unit,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        TotalAmount = x.TotalAmount,
                        IsCompleted = x.IsCompleted,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList()
            };
        }
        #endregion
    }
}
