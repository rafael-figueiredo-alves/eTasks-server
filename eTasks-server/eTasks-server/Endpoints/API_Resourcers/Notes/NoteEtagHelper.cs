using eTasks_server.Models.DTOs.Notes.Requests;
using eTasks_server.Models.DTOs.Notes.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Notes
{
    /// <summary>
    /// Classe utilitária para construir ETags para as respostas de notas, garantindo que as respostas sejam atualizadas corretamente quando os dados subjacentes mudarem.
    /// </summary>
    internal static class NoteEtagHelper
    {
        /// <summary>
        /// Cria um ETag para uma lista de notas com base nos parâmetros de consulta e nos dados das notas. O ETag é gerado a partir de uma combinação dos parâmetros de consulta e dos campos relevantes de cada nota, garantindo que qualquer alteração nos dados ou nos filtros resulte em um ETag diferente.
        /// </summary>
        /// <param name="notes">A lista de notas para a qual o ETag será gerado.</param>
        /// <param name="request">Os parâmetros de consulta usados para filtrar a lista de notas.</param>
        /// <returns>O ETag gerado para a lista de notas.</returns>
        public static string BuildListEtag(IEnumerable<NoteListItemResponse> notes, ListNotesRequest request)
        {
            var builder = new StringBuilder();
            builder.Append(request.SearchTerm ?? string.Empty).Append('|')
                .Append(request.CreatedFrom?.Ticks ?? 0).Append('|')
                .Append(request.CreatedTo?.Ticks ?? 0).Append('|')
                .Append(request.UpdatedFrom?.Ticks ?? 0).Append('|')
                .Append(request.UpdatedTo?.Ticks ?? 0);

            foreach (var note in notes)
            {
                builder.Append('|')
                    .Append(note.Id).Append('|')
                    .Append(note.Subject).Append('|')
                    .Append(note.UpdatedAt?.Ticks ?? note.CreatedAt.Ticks);
            }

            return BuildEtag(builder.ToString());
        }

        /// <summary>
        /// Cria um ETag para os detalhes de uma nota com base nos campos relevantes da nota. O ETag é gerado a partir de uma combinação dos campos da nota, garantindo que qualquer alteração nos dados da nota resulte em um ETag diferente.
        /// </summary>
        /// <param name="note">A nota para a qual o ETag será gerado.</param>
        /// <returns>O ETag gerado para os detalhes da nota.</returns>
        public static string BuildDetailsEtag(NoteDetailsResponse note)
        {
            var content = string.Join('|',
                note.Id,
                note.UserUid,
                note.Subject,
                note.Content,
                note.CreatedAt.Ticks,
                note.UpdatedAt?.Ticks ?? 0);

            return BuildEtag(content);
        }

        /// <summary>
        /// Constrói um ETag a partir de uma string de conteúdo, utilizando o algoritmo SHA256 para garantir que o ETag seja único e consistente para o mesmo conteúdo. O ETag é formatado como uma string hexadecimal entre aspas, seguindo as convenções de ETags HTTP.
        /// </summary>
        /// <param name="content">A string de conteúdo a partir da qual o ETag será gerado.</param>
        /// <returns>O ETag gerado a partir do conteúdo fornecido.</returns>
        private static string BuildEtag(string content)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
