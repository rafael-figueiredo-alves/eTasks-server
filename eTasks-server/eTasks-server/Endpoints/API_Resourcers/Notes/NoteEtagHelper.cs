using eTasks_server.Models.DTOs.Notes.Requests;
using eTasks_server.Models.DTOs.Notes.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Notes
{
    internal static class NoteEtagHelper
    {
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

        private static string BuildEtag(string content)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
