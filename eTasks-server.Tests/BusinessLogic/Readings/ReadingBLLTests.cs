using eTasks_server.Core.BusinessLogicLayers.API_Resources.Readings;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Readings.Requests;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Entities.Readings;
using eTasks_server.Models.Entities.Users;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Readings;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Readings
{
    public class ReadingBLLTests
    {
        [Fact]
        public async Task CreateAsync_CompletedReading_AwardsPoints_AndUsesClientGeneratedId()
        {
            using var context = TestDbContextFactory.Create(nameof(CreateAsync_CompletedReading_AwardsPoints_AndUsesClientGeneratedId));
            var user = TestDbContextFactory.CreateActiveUser();
            context.Users.Add(user);
            context.BonusPointRules.Add(new BonusPointRule
            {
                Source = BonusPointSource.ReadingCompletion,
                Name = "Reading completion",
                DefaultPoints = 12,
                IsActive = true
            });
            await context.SaveChangesAsync();

            IReadingBLL sut = new ReadingBLL(context, NullLogger<IReadingBLL>.Instance);
            var clientId = Guid.CreateVersion7();

            var result = await sut.CreateAsync(user.Uid, new CreateReadingRequest
            {
                ClientGeneratedId = clientId,
                Title = "Clean Code",
                TotalPages = 300,
                CurrentPage = 300,
                Status = ReadingStatus.Completed
            });

            Assert.Equal(clientId, result.Id);
            Assert.Equal(ReadingStatus.Completed, result.Status);
            Assert.Equal(1, context.UserBonusPoints.Count());
            Assert.Equal(BonusPointSource.ReadingCompletion, context.UserBonusPoints.Single().Source);
            Assert.Equal(clientId, context.UserBonusPoints.Single().SourceReferenceId);
        }

        [Fact]
        public async Task DeleteAsync_CompletedReading_CreatesTombstone_AndRevertsPoints()
        {
            using var context = TestDbContextFactory.Create(nameof(DeleteAsync_CompletedReading_CreatesTombstone_AndRevertsPoints));
            var user = TestDbContextFactory.CreateActiveUser();
            var reading = new ReadingItem
            {
                UserUid = user.Uid,
                Title = "Domain-Driven Design",
                TotalPages = 560,
                CurrentPage = 560,
                Status = ReadingStatus.Completed,
                FinishedAt = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.ReadingItems.Add(reading);
            context.UserBonusPoints.Add(new UserBonusPoint
            {
                UserUid = user.Uid,
                Points = 10,
                Source = BonusPointSource.ReadingCompletion,
                SourceReferenceId = reading.Id
            });
            await context.SaveChangesAsync();

            IReadingBLL sut = new ReadingBLL(context, NullLogger<IReadingBLL>.Instance);

            await sut.DeleteAsync(user.Uid, reading.Id);
            var sync = await sut.SyncAsync(user.Uid, new SyncReadingsRequest());

            var stored = context.ReadingItems.Single(x => x.Id == reading.Id);
            Assert.True(stored.IsDeleted);
            Assert.NotNull(stored.DeletedAt);
            Assert.Empty(context.UserBonusPoints);
            Assert.Contains(sync.Deleted, x => x.Id == reading.Id);
        }
    }
}
