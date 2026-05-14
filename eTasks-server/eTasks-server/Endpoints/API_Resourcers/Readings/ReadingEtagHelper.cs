using eTasks_server.Models.DTOs.Readings.Requests;
using eTasks_server.Models.DTOs.Readings.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Readings
{
    /// <summary>
    /// Classe utilitaria para construir ETags das respostas de leituras, garantindo que mudancas nos filtros
    /// ou nos campos relevantes das leituras invalidem corretamente o cache HTTP do cliente.
    /// </summary>
    internal static class ReadingEtagHelper
    {
        /// <summary>
        /// Cria um ETag para uma lista de leituras com base nos filtros da requisicao e nos campos usados para
        /// representar cada item da colecao.
        /// </summary>
        /// <param name="readings">Leituras retornadas pela consulta.</param>
        /// <param name="request">Filtros aplicados na listagem.</param>
        /// <returns>ETag entre aspas para uso no header HTTP ETag.</returns>
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

        /// <summary>
        /// Cria um ETag para os detalhes de uma leitura com base nos campos que compoem a representacao completa
        /// do recurso.
        /// </summary>
        /// <param name="reading">Leitura detalhada.</param>
        /// <returns>ETag entre aspas para uso no header HTTP ETag.</returns>
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

        /// <summary>
        /// Constroi um ETag forte a partir do payload textual informado, usando SHA256 e formato hexadecimal.
        /// </summary>
        /// <param name="payload">Conteudo canonico usado como base do hash.</param>
        /// <returns>Hash SHA256 formatado como ETag HTTP entre aspas.</returns>
        private static string BuildHash(string payload)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
