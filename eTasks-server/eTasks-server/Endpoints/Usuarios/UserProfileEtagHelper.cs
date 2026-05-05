using eTasks_server.Models.DTOs.Users.Profile.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.Usuarios
{
    /// <summary>
    /// Helper para gerar ETags para os dados de perfil do usuário, incluindo configurações e bônus.
    /// </summary>
    public static class UserProfileEtagHelper
    {
        /// <summary>
        /// Gera um ETag para as configurações do usuário.
        /// </summary>
        /// <param name="settings">As configurações do usuário.</param>
        /// <returns>O ETag gerado.</returns>
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

        /// <summary>
        /// Construção de um ETag para os dados de bônus do usuário, considerando o total de pontos, a data da última atualização, as entradas de pontos e as conquistas. O ETag é gerado a partir de uma string que concatena todas essas informações, garantindo que qualquer alteração nos dados resultará em um ETag diferente.
        /// </summary>
        /// <param name="bonus">Os dados de bônus do usuário.</param>
        /// <returns>O ETag gerado.</returns>
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

        /// <summary>
        /// Constrõe um ETag a partir de uma string de payload, utilizando o algoritmo SHA256 para gerar um hash único. O resultado é formatado como uma string hexadecimal e encapsulado entre aspas, seguindo o formato padrão de ETags. Essa abordagem garante que qualquer alteração no conteúdo do payload resultará em um ETag diferente, permitindo uma eficiente validação de cache e controle de versão dos dados.
        /// </summary>
        /// <param name="payload">Carga do etag</param>
        /// <returns>O ETag gerado.</returns>
        private static string BuildHash(string payload)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
