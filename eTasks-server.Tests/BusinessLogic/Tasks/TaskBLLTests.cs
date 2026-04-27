using eTasks_server.Core.BusinessLogicLayers.API_Resources.Tasks;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Tasks.Requests;
using eTasks_server.Models.Entities.Common;
using eTasks_server.Models.Entities.Gamification;
using eTasks_server.Models.Entities.Productivity;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Tasks
{
    public class TaskBLLTests
    {
        [Fact]
        public async Task CreateAsync_WithRecurrence_PersistsTaskRecurrence()
        {
            using var context = TestDbContextFactory.Create(nameof(CreateAsync_WithRecurrence_PersistsTaskRecurrence));
            var user = TestDbContextFactory.CreateActiveUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            ITaskBLL sut = new TaskBLL(context, NullLogger<ITaskBLL>.Instance);

            var taskDate = new DateTime(2026, 4, 28);
            var result = await sut.CreateAsync(user.Uid, new CreateTaskRequest
            {
                Summary = "Pagar contas",
                TaskDate = taskDate,
                Recurrence = new TaskRecurrenceRequest
                {
                    RecurrenceType = RecurrenceType.Weekly,
                    Interval = 1,
                    WeekDays = WeekDays.Monday | WeekDays.Wednesday,
                    StartsOn = taskDate
                }
            });

            Assert.NotNull(result.Recurrence);
            Assert.Single(context.TaskRecurrences);
            Assert.Equal(RecurrenceType.Weekly, context.TaskRecurrences.Single().RecurrenceType);
        }

        [Fact]
        public async Task ListAsync_WithIncludeRecurring_MaterializesGeneratedTask()
        {
            using var context = TestDbContextFactory.Create(nameof(ListAsync_WithIncludeRecurring_MaterializesGeneratedTask));
            var user = TestDbContextFactory.CreateActiveUser();
            var baseTask = new TaskItem
            {
                UserUid = user.Uid,
                Summary = "Treino",
                TaskDate = new DateTime(2026, 4, 28)
            };
            baseTask.Recurrence = new TaskRecurrence
            {
                RecurrenceType = RecurrenceType.Daily,
                Interval = 1,
                StartsOn = baseTask.TaskDate.Date,
                IsActive = true
            };

            context.Users.Add(user);
            context.TaskItems.Add(baseTask);
            await context.SaveChangesAsync();

            ITaskBLL sut = new TaskBLL(context, NullLogger<ITaskBLL>.Instance);

            var list = await sut.ListAsync(user.Uid, new ListTasksRequest
            {
                IncludeRecurring = true,
                ReferenceDate = new DateTime(2026, 4, 29)
            });

            Assert.Single(list);
            Assert.Equal("Treino", list[0].Summary);
            Assert.True(list[0].HasRecurrence);
            Assert.Equal(2, context.TaskItems.Count());
        }

        [Fact]
        public async Task SetCompletionAsync_AwardsAndRevertsCompletionPoints()
        {
            using var context = TestDbContextFactory.Create(nameof(SetCompletionAsync_AwardsAndRevertsCompletionPoints));
            var user = TestDbContextFactory.CreateActiveUser();
            var task = new TaskItem
            {
                UserUid = user.Uid,
                Summary = "Entregar documento",
                TaskDate = new DateTime(2026, 4, 28)
            };

            context.Users.Add(user);
            context.TaskItems.Add(task);
            context.BonusPointRules.Add(new BonusPointRule
            {
                Source = BonusPointSource.TaskCompletion,
                Name = "Task completion",
                DefaultPoints = 5,
                IsActive = true
            });
            await context.SaveChangesAsync();

            ITaskBLL sut = new TaskBLL(context, NullLogger<ITaskBLL>.Instance);

            await sut.SetCompletionAsync(user.Uid, task.Id, true);
            Assert.Single(context.UserBonusPoints);

            await sut.SetCompletionAsync(user.Uid, task.Id, false);
            Assert.Empty(context.UserBonusPoints);
        }
    }
}
