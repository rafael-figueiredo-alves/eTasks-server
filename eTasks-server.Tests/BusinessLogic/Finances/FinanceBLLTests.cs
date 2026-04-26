using eTasks_server.Core.BusinessLogicLayers.API_Resources.Finances;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.Entities.Finances;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Finances
{
    public class FinanceBLLTests
    {
        [Fact]
        public async Task GetMonthSummaryAsync_PositiveBalanceWithRule_IsEligibleForBonus()
        {
            using var context = TestDbContextFactory.Create(nameof(GetMonthSummaryAsync_PositiveBalanceWithRule_IsEligibleForBonus));
            var user = TestDbContextFactory.CreateActiveUser();
            context.Users.Add(user);
            context.FinanceEntries.AddRange(
                new FinanceEntry
                {
                    UserUid = user.Uid,
                    Title = "Salario",
                    EntryType = FinanceEntryType.Credit,
                    PaymentMethod = FinancePaymentMethod.Pix,
                    Amount = 5000,
                    OccursOn = new DateTime(2026, 4, 5),
                    IsPaid = true
                },
                new FinanceEntry
                {
                    UserUid = user.Uid,
                    Title = "Aluguel",
                    EntryType = FinanceEntryType.Debit,
                    PaymentMethod = FinancePaymentMethod.BankTransfer,
                    Amount = 1800,
                    OccursOn = new DateTime(2026, 4, 10),
                    IsPaid = true
                });
            context.BonusPointRules.Add(new BonusPointRule
            {
                Source = BonusPointSource.PositiveMonthlyBalance,
                Name = "Positive balance",
                DefaultPoints = 20,
                IsActive = true
            });
            await context.SaveChangesAsync();

            IFinanceBLL sut = new FinanceBLL(context, NullLogger<IFinanceBLL>.Instance);

            var summary = await sut.GetMonthSummaryAsync(user.Uid, 2026, 4);

            Assert.Equal(5000m, summary.TotalCredits);
            Assert.Equal(1800m, summary.TotalDebits);
            Assert.Equal(3200m, summary.Balance);
            Assert.True(summary.IsPositiveBalance);
            Assert.True(summary.EligibleForBonusPoints);
        }

        [Fact]
        public async Task SyncAsync_DeletedEntry_ReturnsTombstone()
        {
            using var context = TestDbContextFactory.Create(nameof(SyncAsync_DeletedEntry_ReturnsTombstone));
            var user = TestDbContextFactory.CreateActiveUser();
            var entry = new FinanceEntry
            {
                UserUid = user.Uid,
                Title = "Conta",
                EntryType = FinanceEntryType.Debit,
                PaymentMethod = FinancePaymentMethod.Cash,
                Amount = 150,
                OccursOn = new DateTime(2026, 4, 1),
                IsPaid = false,
                IsDeleted = true,
                DeletedAt = new DateTime(2026, 4, 2),
                UpdatedAt = new DateTime(2026, 4, 2)
            };

            context.Users.Add(user);
            context.FinanceEntries.Add(entry);
            await context.SaveChangesAsync();

            IFinanceBLL sut = new FinanceBLL(context, NullLogger<IFinanceBLL>.Instance);

            var sync = await sut.SyncAsync(user.Uid, new SyncFinanceEntriesRequest());

            Assert.Empty(sync.Upserts);
            Assert.Contains(sync.Deleted, x => x.Id == entry.Id);
        }
    }
}
