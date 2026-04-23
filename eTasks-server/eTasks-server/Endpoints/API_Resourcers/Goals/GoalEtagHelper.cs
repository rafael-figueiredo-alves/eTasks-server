using eTasks_server.Models.DTOs.Goals.Requests;
using eTasks_server.Models.DTOs.Goals.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Goals
{
    internal static class GoalEtagHelper
    {
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

        private static string BuildEtag(string content)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
