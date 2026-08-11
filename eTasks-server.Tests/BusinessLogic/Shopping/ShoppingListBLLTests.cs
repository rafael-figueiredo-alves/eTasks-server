using eTasks_server.Core.BusinessLogicLayers.API_Resources.Shopping;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Shopping.Requests;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Entities.Shopping;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Shopping;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Shopping
{
    /// <summary>
    /// Testes unitários para a classe <see cref="ShoppingListBLL"/>.
    /// </summary>
    public class ShoppingListBLLTests
    {
        /// <summary>
        /// Testa se o método UpdateAsync recalcula corretamente os totais e concede pontos de bônus ao finalizar uma lista de compras.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAsync_FinalizingList_RecalculatesTotals_AndAwardsPoints()
        {
            using var context = TestDbContextFactory.Create(nameof(UpdateAsync_FinalizingList_RecalculatesTotals_AndAwardsPoints));
            var user = TestDbContextFactory.CreateActiveUser();
            var list = new ShoppingList
            {
                UserUid = user.Uid,
                Name = "Mercado",
                Type = ShoppingListType.Grocery,
                IsFinalized = false
            };
            var itemId = Guid.CreateVersion7();
            list.Items.Add(new ShoppingListItem
            {
                Id = itemId,
                Description = "Arroz",
                Quantity = 1,
                UnitPrice = 10,
                TotalAmount = 10
            });
            list.TotalItems = 1;
            list.TotalAmount = 10;

            context.Users.Add(user);
            context.ShoppingLists.Add(list);
            context.BonusPointRules.Add(new BonusPointRule
            {
                Source = BonusPointSource.ShoppingListCompletion,
                Name = "Shopping completion",
                DefaultPoints = 7,
                IsActive = true
            });
            await context.SaveChangesAsync();

            IShoppingListBLL sut = new ShoppingListBLL(context, NullLogger<IShoppingListBLL>.Instance);

            var result = await sut.UpdateAsync(user.Uid, list.Id, new UpdateShoppingListRequest
            {
                Name = "Mercado do Mes",
                Type = ShoppingListType.Grocery,
                IsFinalized = true,
                Items =
                [
                    new UpdateShoppingListItemRequest
                    {
                        Id = itemId,
                        Description = "Arroz",
                        Quantity = 2,
                        UnitPrice = 12.5m,
                        Unit = ShoppingItemUnit.Unit,
                        IsCompleted = true
                    }
                ]
            });

            Assert.True(result.IsFinalized);
            Assert.Equal(1, result.TotalItems);
            Assert.Equal(25m, result.TotalAmount);
            Assert.Single(context.UserBonusPoints);
            Assert.Equal(BonusPointSource.ShoppingListCompletion, context.UserBonusPoints.Single().Source);
        }
    }
}
