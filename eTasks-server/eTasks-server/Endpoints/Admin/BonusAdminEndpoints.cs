using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.Admin
{
    public static class BonusAdminEndpoints
    {
        public static IEndpointRouteBuilder MapBonusAdminEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/bonus")
                .WithTags("Gerenciamento de Bônus (Admin)")
                .RequireAuthorization("WebAdmin")
                .ExcludeFromDescription(); // Oculta da documentação pública se desejado

            #region Achievements

            group.MapGet("/achievements", async (IBonusAdminBLL bll) =>
            {
                var achievements = await bll.GetAchievementsAsync();
                return Results.Ok(achievements);
            })
            .WithName("ListBonusAchievements")
            .WithSummary("Lista todas as conquistas de bônus.");

            group.MapGet("/achievements/{id:guid}", async (Guid id, IBonusAdminBLL bll) =>
            {
                var achievement = await bll.GetAchievementAsync(id);
                return Results.Ok(achievement);
            })
            .WithName("GetBonusAchievement")
            .WithSummary("Obtém uma conquista de bônus específica.");

            group.MapPost("/achievements", async ([FromBody] BonusAchievementRequest request, IBonusAdminBLL bll) =>
            {
                var achievement = await bll.CreateAchievementAsync(request);
                return Results.Created($"/api/v2/bonus/achievements/{achievement.Id}", achievement);
            })
            .WithName("CreateBonusAchievement")
            .WithSummary("Cria uma nova conquista de bônus.");

            group.MapPut("/achievements/{id:guid}", async (Guid id, [FromBody] BonusAchievementRequest request, IBonusAdminBLL bll) =>
            {
                var achievement = await bll.UpdateAchievementAsync(id, request);
                return Results.Ok(achievement);
            })
            .WithName("UpdateBonusAchievement")
            .WithSummary("Atualiza uma conquista de bônus existente.");

            group.MapDelete("/achievements/{id:guid}", async (Guid id, IBonusAdminBLL bll) =>
            {
                await bll.DeleteAchievementAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteBonusAchievement")
            .WithSummary("Remove uma conquista de bônus (se não houver vínculos).");

            #endregion

            #region Rules

            group.MapGet("/rules", async (IBonusAdminBLL bll) =>
            {
                var rules = await bll.GetRulesAsync();
                return Results.Ok(rules);
            })
            .WithName("ListBonusRules")
            .WithSummary("Lista todas as regras de pontuação de bônus.");

            group.MapGet("/rules/{id:guid}", async (Guid id, IBonusAdminBLL bll) =>
            {
                var rule = await bll.GetRuleAsync(id);
                return Results.Ok(rule);
            })
            .WithName("GetBonusRule")
            .WithSummary("Obtém uma regra de pontuação específica.");

            group.MapPost("/rules", async ([FromBody] BonusPointRuleCreateRequest request, IBonusAdminBLL bll) =>
            {
                var rule = await bll.CreateRuleAsync(request);
                return Results.Created($"/api/v2/bonus/rules/{rule.Id}", rule);
            })
            .WithName("CreateBonusRule")
            .WithSummary("Cria uma nova regra de pontuação (Origin deve ser único).");

            group.MapPut("/rules/{id:guid}", async (Guid id, [FromBody] BonusPointRuleUpdateRequest request, IBonusAdminBLL bll) =>
            {
                var rule = await bll.UpdateRuleAsync(id, request);
                return Results.Ok(rule);
            })
            .WithName("UpdateBonusRule")
            .WithSummary("Atualiza os dados de uma regra de pontuação (excluindo Origin).");

            group.MapDelete("/rules/{id:guid}", async (Guid id, IBonusAdminBLL bll) =>
            {
                await bll.DeleteRuleAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteBonusRule")
            .WithSummary("Remove uma regra de pontuação de bônus.");

            #endregion

            return app;
        }
    }
}
