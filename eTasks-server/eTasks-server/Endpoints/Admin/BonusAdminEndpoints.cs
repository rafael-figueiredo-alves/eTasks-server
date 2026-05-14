using eTasks_server.Core.BusinessLogicLayers.Interfaces;
using eTasks_server.Models.DTOs.Gamification.BonusAchievement.Requests;
using eTasks_server.Models.DTOs.Gamification.BonusPointRule.Requests;
using Microsoft.AspNetCore.Mvc;

namespace eTasks_server.Endpoints.Admin
{
    public static class BonusAdminEndpoints
    {
        extension(IEndpointRouteBuilder app)
        {
            /// <summary>
            /// Mapeia os endpoints relacionados ao gerenciamento de bônus para administradores. Esses endpoints permitem a criação, leitura, atualização e exclusão de conquistas de bônus e regras de pontuação. O acesso a esses endpoints é restrito a usuários com a autorização "WebAdmin". Os endpoints são organizados em um grupo com a tag "Gerenciamento de Bônus (Admin)" para facilitar a identificação na documentação da API.
            /// </summary>
            /// <param name="app"></param>
            /// <returns></returns>
            public IEndpointRouteBuilder MapBonusAdminEndpoints()
            {
                var group = app.MapGroup("/bonus")
                    .WithTags("Gerenciamento de Bônus (Admin)")
                    .RequireAuthorization("WebAdmin")
                    .ExcludeFromDescription(); // Oculta da documentação pública se desejado

                group
                #region Conquistas
                     .ListarConquistas()
                     .ObterConquista()
                     .CriarConquista()
                     .AtualizarConquista()
                     .DeletarConquista()
                #endregion

                #region Regras
                     .ListarRegras()
                     .ObterRegra()
                     .CriarRegra()
                     .AtualizarRegra()
                     .DeletarRegra();
                #endregion

                return app;
            }
        }

        extension(RouteGroupBuilder group)
        {
            #region Achievements

            /// <summary>
            /// Lista todas as conquistas de bônus cadastradas no sistema, incluindo detalhes como nome, descrição, pontos concedidos e critérios de obtenção.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder ListarConquistas()
            {
                group.MapGet("/achievements", async (IBonusAdminBLL bll) =>
                {
                    var achievements = await bll.GetAchievementsAsync();
                    return Results.Ok(achievements);
                })
                .WithName("ListBonusAchievements")
                .WithSummary("Lista todas as conquistas de bônus.");

                return group;
            }

            /// <summary>
            /// Obtem dados de uma conquista de bônus específica, identificada por seu ID. Retorna detalhes como nome, descrição, pontos concedidos e critérios de obtenção.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder ObterConquista()
            {
                group.MapGet("/achievements/{id:guid}", async (Guid id, IBonusAdminBLL bll) =>
                {
                    var achievement = await bll.GetAchievementAsync(id);
                    return Results.Ok(achievement);
                })
                .WithName("GetBonusAchievement")
                .WithSummary("Obtém uma conquista de bônus específica.");

                return group;
            }

            /// <summary>
            /// Cria nova conquista de bônus com os dados fornecidos. O sistema atribuirá um ID único à conquista criada. Retorna os detalhes da conquista criada, incluindo seu ID.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder CriarConquista()
            {
                group.MapPost("/achievements", async ([FromBody] BonusAchievementRequest request, IBonusAdminBLL bll) =>
                {
                    var achievement = await bll.CreateAchievementAsync(request);
                    return Results.Created($"/api/v2/bonus/achievements/{achievement.Id}", achievement);
                })
                .WithName("CreateBonusAchievement")
                .WithSummary("Cria uma nova conquista de bônus.");

                return group;
            }

            /// <summary>
            /// Realiza a atualização dos dados de uma conquista de bônus existente, identificada por seu ID. Permite modificar campos como nome, descrição, pontos concedidos e critérios de obtenção. Retorna os detalhes atualizados da conquista.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder AtualizarConquista()
            {
                group.MapPut("/achievements/{id:guid}", async (Guid id, [FromBody] BonusAchievementRequest request, IBonusAdminBLL bll) =>
                {
                    var achievement = await bll.UpdateAchievementAsync(id, request);
                    return Results.Ok(achievement);
                })
                .WithName("UpdateBonusAchievement")
                .WithSummary("Atualiza uma conquista de bônus existente.");

                return group;
            }

            /// <summary>
            /// Método para remover uma conquista de bônus do sistema, identificada por seu ID. A exclusão só será permitida se a conquista não estiver vinculada a nenhum usuário ou regra de pontuação. Retorna status de sucesso ou erro caso haja vínculos existentes.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder DeletarConquista()            
            {
                group.MapDelete("/achievements/{id:guid}", async (Guid id, IBonusAdminBLL bll) =>
                {
                    await bll.DeleteAchievementAsync(id);
                    return Results.NoContent();
                })
                .WithName("DeleteBonusAchievement")
                .WithSummary("Remove uma conquista de bônus (se não houver vínculos).");

                return group;
            }
            #endregion

            #region Rules

            /// <summary>
            /// Método para listar todas as regras de pontuação de bônus cadastradas no sistema, incluindo detalhes como nome, descrição, pontos concedidos e critérios de aplicação. Permite aos administradores visualizar e gerenciar as regras existentes.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder ListarRegras()
            {
                group.MapGet("/rules", async (IBonusAdminBLL bll) =>
                {
                    var rules = await bll.GetRulesAsync();
                    return Results.Ok(rules);
                })
                .WithName("ListBonusRules")
                .WithSummary("Lista todas as regras de pontuação de bônus.");

                return group;
            }

            /// <summary>
            /// Método para obter os detalhes de uma regra de pontuação de bônus específica, identificada por seu ID. Retorna informações como nome, descrição, pontos concedidos e critérios de aplicação da regra. Permite aos administradores visualizar os detalhes de uma regra específica para fins de gerenciamento.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder ObterRegra()
            {
                group.MapGet("/rules/{id:guid}", async (Guid id, IBonusAdminBLL bll) =>
                                                 {
                                                    var rule = await bll.GetRuleAsync(id);
                                                    return Results.Ok(rule);
                                                 })
                .WithName("GetBonusRule")
                .WithSummary("Obtém uma regra de pontuação específica.");

                return group;
            }

            /// <summary>
            /// Cria uma nova regra de pontuação de bônus com os dados fornecidos. O sistema atribuirá um ID único à regra criada. Retorna os detalhes da regra criada, incluindo seu ID. A origem (Origin) da regra deve ser única para evitar conflitos na aplicação das regras de pontuação.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder CriarRegra()
            {
                group.MapPost("/rules", async ([FromBody] BonusPointRuleCreateRequest request, IBonusAdminBLL bll) =>
                {
                    var rule = await bll.CreateRuleAsync(request);
                    return Results.Created($"/api/v2/bonus/rules/{rule.Id}", rule);
                })
                .WithName("CreateBonusRule")
                .WithSummary("Cria uma nova regra de pontuação (Origin deve ser único).");

                return group;
            }

            /// <summary>
            /// Atualiza os dados de uma regra de pontuação de bônus existente, identificada por seu ID. Permite modificar campos como nome, descrição, pontos concedidos e critérios de aplicação da regra. A origem (Origin) da regra não pode ser alterada para garantir a consistência na aplicação das regras de pontuação. Retorna os detalhes atualizados da regra.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder AtualizarRegra()
            {
                group.MapPut("/rules/{id:guid}", async (Guid id, [FromBody] BonusPointRuleUpdateRequest request, IBonusAdminBLL bll) =>
                {
                    var rule = await bll.UpdateRuleAsync(id, request);
                    return Results.Ok(rule);
                })
                .WithName("UpdateBonusRule")
                .WithSummary("Atualiza os dados de uma regra de pontuação (excluindo Origin).");

                return group;
            }

            /// <summary>
            /// Método para remover uma regra de pontuação de bônus do sistema, identificada por seu ID. A exclusão só será permitida se a regra não estiver vinculada a nenhum usuário ou conquista. Retorna status de sucesso ou erro caso haja vínculos existentes. Permite aos administradores manter o sistema organizado e livre de regras obsoletas ou desnecessárias.
            /// </summary>
            /// <returns></returns>
            private RouteGroupBuilder DeletarRegra()
            {
                group.MapDelete("/rules/{id:guid}", async (Guid id, IBonusAdminBLL bll) =>
                {
                    await bll.DeleteRuleAsync(id);
                    return Results.NoContent();
                })
                .WithName("DeleteBonusRule")
                .WithSummary("Remove uma regra de pontuação de bônus.");

                return group;
            }
            #endregion
        }
    }
}
