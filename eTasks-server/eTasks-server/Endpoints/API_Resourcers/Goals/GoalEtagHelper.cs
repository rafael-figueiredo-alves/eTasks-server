using eTasks_server.Models.DTOs.Goals.Requests;
using eTasks_server.Models.DTOs.Goals.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Goals
{
    /// <summary>
    /// Classe utilitaria para construir ETags das respostas de metas, garantindo que mudancas nos filtros
    /// ou nos campos relevantes das metas invalidem corretamente o cache HTTP do cliente.
    /// </summary>
    internal static class GoalEtagHelper
    {
        /// <summary>
        /// Cria um ETag para uma lista de metas com base nos filtros da requisicao e nos campos usados para
        /// representar cada item da colecao.
        /// </summary>
        /// <param name="goals">Metas retornadas pela consulta.</param>
        /// <param name="request">Filtros aplicados na listagem.</param>
        /// <returns>ETag entre aspas para uso no header HTTP ETag.</returns>
        public static string BuildListEtag(IEnumerable<GoalListItemResponse> goals, ListGoalsRequest request)
        {
            var builder = new StringBuilder();
            builder.Append(request.Status?.ToString() ?? "null").Append('|')
                .Append(request.Type?.ToString() ?? "null").Append('|')
                .Append(request.Priority?.ToString() ?? "null").Append('|')
                .Append(request.OnlyRewarded?.ToString() ?? "null").Append('|')
                .Append(request.SearchTerm ?? string.Empty);

            foreach (var goal in goals)
            {
                builder.Append('|')
                    .Append(goal.Id).Append('|')
                    .Append(goal.Summary).Append('|')
                    .Append(goal.Status).Append('|')
                    .Append(goal.Priority).Append('|')
                    .Append(goal.UpdatedAt?.Ticks ?? goal.CreatedAt.Ticks);
            }

            return BuildEtag(builder.ToString());
        }

        /// <summary>
        /// Cria um ETag para os detalhes de uma meta com base nos campos que compoem a representacao completa
        /// do recurso.
        /// </summary>
        /// <param name="goal">Meta detalhada.</param>
        /// <returns>ETag entre aspas para uso no header HTTP ETag.</returns>
        public static string BuildDetailsEtag(GoalDetailsResponse goal)
        {
            var content = string.Join('|',
                goal.Id,
                goal.UserUid,
                goal.Summary,
                goal.Description ?? string.Empty,
                goal.Type,
                goal.Priority,
                goal.RewardPoints?.ToString() ?? string.Empty,
                goal.Status,
                goal.CreatedAt.Ticks,
                goal.UpdatedAt?.Ticks ?? 0);

            return BuildEtag(content);
        }

        /// <summary>
        /// Constroi um ETag forte a partir do conteudo textual informado, usando SHA256 e formato hexadecimal.
        /// </summary>
        /// <param name="content">Conteudo canonico usado como base do hash.</param>
        /// <returns>Hash SHA256 formatado como ETag HTTP entre aspas.</returns>
        private static string BuildEtag(string content)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
