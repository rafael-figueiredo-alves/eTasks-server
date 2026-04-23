using eTasks_server.Models.DTOs.Notes.Requests;
using eTasks_server.Models.DTOs.Notes.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    public interface INoteBLL
    {
        Task<List<NoteListItemResponse>> ListAsync(Guid userUid, ListNotesRequest request, CancellationToken cancellationToken = default);
        Task<NoteDetailsResponse> GetByIdAsync(Guid userUid, Guid noteId, CancellationToken cancellationToken = default);
        Task<NoteDetailsResponse> CreateAsync(Guid userUid, CreateNoteRequest request, CancellationToken cancellationToken = default);
        Task<NoteDetailsResponse> UpdateAsync(Guid userUid, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid userUid, Guid noteId, CancellationToken cancellationToken = default);
        Task<NoteSyncResponse> SyncAsync(Guid userUid, SyncNotesRequest request, CancellationToken cancellationToken = default);
    }
}
