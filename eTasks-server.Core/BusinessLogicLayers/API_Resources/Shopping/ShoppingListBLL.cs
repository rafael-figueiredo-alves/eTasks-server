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
    public class ShoppingListBLL(AppDbContext context, ILogger<IShoppingListBLL> logger) : BaseBLL<IShoppingListBLL>(context, logger), IShoppingListBLL
    {
        public async Task<List<ShoppingListListItemResponse>> ListAsync(Guid userUid, ListShoppingListsRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            NormalizeListRequest(request);
            ValidateListRequest(request);

            var query = _context.ShoppingLists
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            if (request.IsFinalized.HasValue)
            {
                query = query.Where(x => x.IsFinalized == request.IsFinalized.Value);
            }

            if (request.Type.HasValue)
            {
                query = query.Where(x => x.Type == request.Type.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Place))
            {
                query = query.Where(x => x.Place != null && x.Place.Contains(request.Place));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(x => x.Name.Contains(request.SearchTerm));
            }

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

        public async Task<ShoppingListDetailsResponse> GetByIdAsync(Guid userUid, Guid shoppingListId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var list = await _context.ShoppingLists
                .AsNoTracking()
                .Include(x => x.Items.OrderBy(item => item.CreatedAt))
                .FirstOrDefaultAsync(x => x.Id == shoppingListId && !x.IsDeleted, cancellationToken);

            list = EnsureFound(list, "Lista de compras nao encontrada.");
            EnsureOwnership(list.UserUid, userUid);

            return MapDetails(list);
        }

        public async Task<ShoppingListDetailsResponse> CreateAsync(Guid userUid, CreateShoppingListRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Name, request.Place, request.Type, request.Items);
            await ValidateClientGeneratedIdAsync(request.ClientGeneratedId, cancellationToken);

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

            foreach (var itemRequest in request.Items)
            {
                list.Items.Add(MapNewItem(itemRequest));
            }

            RecalculateTotals(list);

            await ExecuteInTransactionAsync(async () =>
            {
                await _context.ShoppingLists.AddAsync(list, cancellationToken);
                await SaveChangesContextAsync(cancellationToken);

                if (list.IsFinalized)
                {
                    await AwardCompletionPointsAsync(list, cancellationToken);
                    await SaveChangesContextAsync(cancellationToken);
                }
            });

            return await GetByIdAsync(userUid, list.Id, cancellationToken);
        }

        public async Task<ShoppingListDetailsResponse> UpdateAsync(Guid userUid, Guid shoppingListId, UpdateShoppingListRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);
            ValidatePayload(request.Name, request.Place, request.Type, request.Items.Select(x => new CreateShoppingListItemRequest
            {
                ClientGeneratedId = x.Id,
                Description = x.Description,
                Unit = x.Unit,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                IsCompleted = x.IsCompleted
            }).ToList());

            var list = await _context.ShoppingLists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == shoppingListId && !x.IsDeleted, cancellationToken);

            list = EnsureFound(list, "Lista de compras nao encontrada.");
            EnsureOwnership(list.UserUid, userUid);

            var wasFinalized = list.IsFinalized;

            list.Name = request.Name.Trim();
            list.Place = NormalizeText(request.Place);
            list.Type = request.Type;
            list.IsFinalized = request.IsFinalized;
            list.UpdatedAt = SaoPauloDateTime.Now();

            ApplyItemDiff(list, request.Items);
            RecalculateTotals(list);

            await ExecuteInTransactionAsync(async () =>
            {
                if (!wasFinalized && list.IsFinalized)
                {
                    await AwardCompletionPointsAsync(list, cancellationToken);
                }
                else if (wasFinalized && !list.IsFinalized)
                {
                    await RevertCompletionPointsAsync(list, cancellationToken);
                }

                await SaveChangesContextAsync(cancellationToken);
            });

            return await GetByIdAsync(userUid, list.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid userUid, Guid shoppingListId, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var list = await _context.ShoppingLists.FirstOrDefaultAsync(x => x.Id == shoppingListId && !x.IsDeleted, cancellationToken);
            list = EnsureFound(list, "Lista de compras nao encontrada.");
            EnsureOwnership(list.UserUid, userUid);

            await ExecuteInTransactionAsync(async () =>
            {
                await RevertCompletionPointsAsync(list, cancellationToken);
                var deletedAt = SaoPauloDateTime.Now();
                list.IsDeleted = true;
                list.DeletedAt = deletedAt;
                list.UpdatedAt = deletedAt;
                await SaveChangesContextAsync(cancellationToken);
            });
        }

        public async Task<ShoppingListSyncResponse> SyncAsync(Guid userUid, SyncShoppingListsRequest request, CancellationToken cancellationToken = default)
        {
            await GetAndValidateActiveUserAsync(userUid);

            var upsertsQuery = _context.ShoppingLists
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.UserUid == userUid && !x.IsDeleted);

            var deletedQuery = _context.ShoppingLists
                .AsNoTracking()
                .Where(x => x.UserUid == userUid && x.IsDeleted && x.DeletedAt.HasValue);

            if (request.Since.HasValue)
            {
                upsertsQuery = upsertsQuery.Where(x => (x.UpdatedAt ?? x.CreatedAt) > request.Since.Value);
                deletedQuery = deletedQuery.Where(x => x.DeletedAt!.Value > request.Since.Value);
            }

            var upserts = await upsertsQuery.OrderBy(x => x.Name).ThenBy(x => x.Id).ToListAsync(cancellationToken);
            var deleted = await deletedQuery.OrderBy(x => x.DeletedAt).ThenBy(x => x.Id)
                .Select(x => new DeletedShoppingListResponse { Id = x.Id, DeletedAt = x.DeletedAt!.Value })
                .ToListAsync(cancellationToken);

            return new ShoppingListSyncResponse
            {
                ServerTime = SaoPauloDateTime.Now(),
                Upserts = upserts.Select(MapDetails).ToList(),
                Deleted = deleted
            };
        }

        private static void NormalizeListRequest(ListShoppingListsRequest request)
        {
            request.Place = string.IsNullOrWhiteSpace(request.Place) ? null : request.Place.Trim();
            request.SearchTerm = string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim();
        }

        private static void ValidateListRequest(ListShoppingListsRequest request)
        {
            if (request.Type.HasValue && !Enum.IsDefined(request.Type.Value))
            {
                throw new ValidationException("Type", "Tipo da lista de compras invalido.");
            }
        }

        private static void ValidatePayload(string name, string? place, ShoppingListType type, IEnumerable<CreateShoppingListItemRequest> items)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ValidationException("Name", "O nome da lista e obrigatorio.");
            }

            if (name.Trim().Length > 200)
            {
                throw new ValidationException("Name", "O nome da lista deve ter no maximo 200 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(place) && place.Trim().Length > 200)
            {
                throw new ValidationException("Place", "O local deve ter no maximo 200 caracteres.");
            }

            if (!Enum.IsDefined(type))
            {
                throw new ValidationException("Type", "Tipo da lista de compras invalido.");
            }

            var ids = new HashSet<Guid>();
            foreach (var item in items)
            {
                ValidateItem(item.Description, item.Unit, item.Quantity, item.UnitPrice);

                if (item.ClientGeneratedId.HasValue && !ids.Add(item.ClientGeneratedId.Value))
                {
                    throw new ValidationException("Items", "Nao e permitido repetir o identificador de item na mesma lista.");
                }
            }
        }

        private static void ValidateItem(string description, ShoppingItemUnit unit, decimal quantity, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ValidationException("Description", "A descricao do item e obrigatoria.");
            }

            if (description.Trim().Length > 200)
            {
                throw new ValidationException("Description", "A descricao do item deve ter no maximo 200 caracteres.");
            }

            if (!Enum.IsDefined(unit))
            {
                throw new ValidationException("Unit", "Unidade do item invalida.");
            }

            if (quantity <= 0)
            {
                throw new ValidationException("Quantity", "A quantidade do item deve ser maior que zero.");
            }

            if (unitPrice < 0)
            {
                throw new ValidationException("UnitPrice", "O preco unitario nao pode ser negativo.");
            }
        }

        private async Task ValidateClientGeneratedIdAsync(Guid? clientGeneratedId, CancellationToken cancellationToken)
        {
            if (!clientGeneratedId.HasValue)
            {
                return;
            }

            var alreadyExists = await _context.ShoppingLists.AnyAsync(x => x.Id == clientGeneratedId.Value, cancellationToken);
            if (alreadyExists)
            {
                throw new ValidationException("ClientGeneratedId", "Ja existe uma lista com o identificador informado pelo cliente.");
            }
        }

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

        private static void ApplyItemDiff(ShoppingList list, ICollection<UpdateShoppingListItemRequest> items)
        {
            var existingById = list.Items.ToDictionary(x => x.Id);
            var requestedIds = new HashSet<Guid>();

            foreach (var itemRequest in items)
            {
                ValidateItem(itemRequest.Description, itemRequest.Unit, itemRequest.Quantity, itemRequest.UnitPrice);

                if (!requestedIds.Add(itemRequest.Id))
                {
                    throw new ValidationException("Items", "Nao e permitido repetir o item dentro da mesma lista.");
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

        private static void RecalculateTotals(ShoppingList list)
        {
            list.TotalItems = list.Items.Count;
            list.TotalAmount = decimal.Round(list.Items.Sum(x => x.TotalAmount), 2, MidpointRounding.AwayFromZero);
        }

        private async Task AwardCompletionPointsAsync(ShoppingList list, CancellationToken cancellationToken)
        {
            var alreadyExists = await _context.UserBonusPoints.AnyAsync(x =>
                x.UserUid == list.UserUid &&
                x.Source == BonusPointSource.ShoppingListCompletion &&
                x.SourceReferenceId == list.Id, cancellationToken);

            if (alreadyExists)
            {
                return;
            }

            var rule = await _context.BonusPointRules.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Source == BonusPointSource.ShoppingListCompletion && x.IsActive, cancellationToken);

            if (rule is null || rule.DefaultPoints <= 0)
            {
                return;
            }

            await _context.UserBonusPoints.AddAsync(new UserBonusPoint
            {
                UserUid = list.UserUid,
                Points = rule.DefaultPoints,
                Source = BonusPointSource.ShoppingListCompletion,
                SourceReferenceId = list.Id,
                Description = $"Finalizacao da lista de compras '{list.Name}'."
            }, cancellationToken);
        }

        private async Task RevertCompletionPointsAsync(ShoppingList list, CancellationToken cancellationToken)
        {
            var entries = await _context.UserBonusPoints.Where(x =>
                x.UserUid == list.UserUid &&
                x.Source == BonusPointSource.ShoppingListCompletion &&
                x.SourceReferenceId == list.Id).ToListAsync(cancellationToken);

            if (entries.Count > 0)
            {
                _context.UserBonusPoints.RemoveRange(entries);
            }
        }

        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
    }
}
