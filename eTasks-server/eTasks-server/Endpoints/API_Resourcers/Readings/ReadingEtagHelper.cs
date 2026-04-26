using eTasks_server.Models.DTOs.Readings.Requests;
using eTasks_server.Models.DTOs.Readings.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Readings
{
    internal static class ReadingEtagHelper
    {
        public static string BuildListEtag(IEnumerable<ReadingListItemResponse> readings, ListReadingsRequest request)
        {
            var builder = new StringBuilder();
            builder.Append(request.Status?.ToString() ?? string.Empty).Append('|')
                .Append(request.Format?.ToString() ?? string.Empty).Append('|')
                .Append(request.Genre ?? string.Empty).Append('|')
                .Append(request.RatingFrom?.ToString() ?? string.Empty).Append('|')
                .Append(request.RatingTo?.ToString() ?? string.Empty).Append('|')
                .Append(request.StartedFrom?.Ticks ?? 0).Append('|')
                .Append(request.StartedTo?.Ticks ?? 0).Append('|')
                .Append(request.SearchTerm ?? string.Empty);

            foreach (var reading in readings.OrderBy(x => x.Title).ThenBy(x => x.Id))
            {
                builder.Append('|')
                    .Append(reading.Id)
                    .Append('|').Append(reading.Title)
                    .Append('|').Append(reading.CurrentPage)
                    .Append('|').Append(reading.TotalPages)
                    .Append('|').Append(reading.Status)
                    .Append('|').Append(reading.FinishedAt?.Ticks ?? 0);
            }

            return BuildHash(builder.ToString());
        }

        public static string BuildDetailsEtag(ReadingDetailsResponse reading)
        {
            var payload = string.Join('|',
                reading.Id,
                reading.UserUid,
                reading.Title,
                reading.Authors ?? string.Empty,
                reading.Subject ?? string.Empty,
                reading.Summary ?? string.Empty,
                reading.Opinion ?? string.Empty,
                reading.Rating?.ToString() ?? string.Empty,
                reading.TotalPages,
                reading.CurrentPage,
                reading.Genre ?? string.Empty,
                reading.Format,
                reading.Status,
                reading.StartedAt?.Ticks ?? 0,
                reading.FinishedAt?.Ticks ?? 0,
                reading.CreatedAt.Ticks,
                reading.UpdatedAt?.Ticks ?? 0);

            return BuildHash(payload);
        }

        private static string BuildHash(string payload)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
