using eTasks_server.Core.BusinessLogicLayers.API_Resources.Finances;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.Entities.Common;
using eTasks_server.Models.Entities.Finances;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Tests.Support;
using Microsoft.EntityFrameworkCore;
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

        [Fact]
        public async Task CreateAsync_WithRecurrence_PersistsFinanceRecurrence()
        {
            using var context = TestDbContextFactory.Create(nameof(CreateAsync_WithRecurrence_PersistsFinanceRecurrence));
            var user = TestDbContextFactory.CreateActiveUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            IFinanceBLL sut = new FinanceBLL(context, NullLogger<IFinanceBLL>.Instance);

            var result = await sut.CreateAsync(user.Uid, new CreateFinanceEntryRequest
            {
                Title = "Internet",
                EntryType = FinanceEntryType.Debit,
                PaymentMethod = FinancePaymentMethod.CreditCard,
                Amount = 199.90m,
                OccursOn = new DateTime(2026, 4, 15),
                IsPaid = false,
                IsRecurring = true,
                Recurrence = new FinanceRecurrenceRequest
                {
                    RecurrenceType = RecurrenceType.Monthly,
                    RecurrenceInterval = 1,
                    DayOfMonth = 15
                }
            });

            var recurrence = await context.FinanceRecurrences.SingleAsync();

            Assert.NotNull(result.Recurrence);
            Assert.Equal(result.Id, recurrence.FinanceEntryId);
            Assert.Equal(RecurrenceType.Monthly, recurrence.RecurrenceType);
            Assert.Equal(1, recurrence.Interval);
            Assert.Equal(15, recurrence.DayOfMonth);
        }

        [Fact]
        public async Task UpdateAsync_DisablingRecurrence_RemovesFinanceRecurrence()
        {
            using var context = TestDbContextFactory.Create(nameof(UpdateAsync_DisablingRecurrence_RemovesFinanceRecurrence));
            var user = TestDbContextFactory.CreateActiveUser();
            var entry = new FinanceEntry
            {
                UserUid = user.Uid,
                Title = "Academia",
                EntryType = FinanceEntryType.Debit,
                PaymentMethod = FinancePaymentMethod.Pix,
                Amount = 99.90m,
                OccursOn = new DateTime(2026, 4, 8),
                IsPaid = false,
                IsRecurring = true,
                Recurrence = new FinanceRecurrence
                {
                    RecurrenceType = RecurrenceType.Monthly,
                    Interval = 1,
                    DayOfMonth = 8
                }
            };

            context.Users.Add(user);
            context.FinanceEntries.Add(entry);
            await context.SaveChangesAsync();

            IFinanceBLL sut = new FinanceBLL(context, NullLogger<IFinanceBLL>.Instance);

            var result = await sut.UpdateAsync(user.Uid, entry.Id, new UpdateFinanceEntryRequest
            {
                Title = entry.Title,
                EntryType = entry.EntryType,
                PaymentMethod = entry.PaymentMethod,
                Amount = entry.Amount,
                OccursOn = entry.OccursOn,
                IsPaid = entry.IsPaid,
                IsRecurring = false,
                Recurrence = null
            });

            Assert.False(result.IsRecurring);
            Assert.Null(result.Recurrence);
            Assert.Empty(context.FinanceRecurrences);
        }
    }
}
