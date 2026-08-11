using eTasks_server.Core.BusinessLogicLayers.API_Resources.Goals;
using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Goals.Requests;
using eTasks_server.Models.Enums.Bonus;
using eTasks_server.Models.Enums.Goals;
using eTasks_server.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace eTasks_server.Tests.BusinessLogic.Goals
{
    /// <summary>
    /// Testa a classe GoalBLL, que é responsável por gerenciar as metas dos usuários.
    /// </summary>
    public class GoalBLLTests
    {
        /// <summary>
        /// Testa se ao criar uma meta com status "Completed", os pontos de recompensa configurados são concedidos ao usuário.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateAsync_WithCompletedStatus_AwardsConfiguredRewardPoints()
        {
            using var context = TestDbContextFactory.Create(nameof(CreateAsync_WithCompletedStatus_AwardsConfiguredRewardPoints));
            var user = TestDbContextFactory.CreateActiveUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            IGoalBLL sut = new GoalBLL(context, NullLogger<IGoalBLL>.Instance);

            var result = await sut.CreateAsync(user.Uid, new CreateGoalRequest
            {
                Summary = "Completar curso",
                Status = GoalStatus.Completed,
                RewardPoints = 17
            });

            Assert.Equal(GoalStatus.Completed, result.Status);
            Assert.Single(context.UserBonusPoints);
            Assert.Equal(17, context.UserBonusPoints.Single().Points);
        }

        /// <summary>
        /// Testa se ao atualizar uma meta de "Completed" para "Active", os pontos de recompensa concedidos anteriormente são revertidos.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAsync_ReopeningCompletedGoal_RevertsPoints()
        {
            using var context = TestDbContextFactory.Create(nameof(UpdateAsync_ReopeningCompletedGoal_RevertsPoints));
            var user = TestDbContextFactory.CreateActiveUser();
            var goal = new Models.Entities.Goals.Goal
            {
                UserUid = user.Uid,
                Summary = "Meta A",
                Status = GoalStatus.Completed,
                RewardPoints = 10
            };

            context.Users.Add(user);
            context.Goals.Add(goal);
            context.UserBonusPoints.Add(new Models.Entities.Users.UserBonusPoint
            {
                UserUid = user.Uid,
                Points = 10,
                Source = BonusPointSource.GoalCompletion,
                SourceReferenceId = goal.Id
            });
            await context.SaveChangesAsync();

            IGoalBLL sut = new GoalBLL(context, NullLogger<IGoalBLL>.Instance);

            var result = await sut.UpdateAsync(user.Uid, goal.Id, new UpdateGoalRequest
            {
                Summary = goal.Summary,
                Status = GoalStatus.Active
            });

            Assert.Equal(GoalStatus.Active, result.Status);
            Assert.Empty(context.UserBonusPoints);
        }
    }
}
