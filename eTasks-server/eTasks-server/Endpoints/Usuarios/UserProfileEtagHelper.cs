using eTasks_server.Models.DTOs.Users.Profile.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.Usuarios
{
    internal static class UserProfileEtagHelper
    {
        public static string BuildSettingsEtag(UserSettingsSyncResponse settings)
        {
            var payload = string.Join('|',
                settings.Id,
                settings.UserUid,
                settings.Theme,
                settings.Language,
                settings.InitialScreen,
                settings.EnableBonusSystem,
                settings.CreatedAt.Ticks,
                settings.UpdatedAt.Ticks);

            return BuildHash(payload);
        }

        public static string BuildBonusEtag(UserBonusSyncResponse bonus)
        {
            var builder = new StringBuilder();
            builder.Append(bonus.TotalPoints).Append('|').Append(bonus.LastUpdatedAt.Ticks);

            foreach (var point in bonus.PointEntries.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id))
            {
                builder.Append('|')
                    .Append(point.Id)
                    .Append('|').Append(point.Points)
                    .Append('|').Append(point.Source)
                    .Append('|').Append(point.CreatedAt.Ticks);
            }

            foreach (var achievement in bonus.Achievements.OrderByDescending(x => x.AchievedAt).ThenBy(x => x.Code))
            {
                builder.Append('|')
                    .Append(achievement.Code)
                    .Append('|').Append(achievement.Name)
                    .Append('|').Append(achievement.PointsRequired)
                    .Append('|').Append(achievement.DisplayType)
                    .Append('|').Append(achievement.AchievedAt.Ticks);
            }

            return BuildHash(builder.ToString());
        }

        private static string BuildHash(string payload)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
