using eTasks_server.Models.DTOs.Notes.Requests;
using eTasks_server.Models.DTOs.Notes.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Interface de anotações
    /// </summary>
    public interface INoteBLL
    {
        /// <summary>
        /// Obtem lista de anotações
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<NoteListItemResponse>> ListAsync(Guid userUid, ListNotesRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem anotação pelo id fornecido
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="noteId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<NoteDetailsResponse> GetByIdAsync(Guid userUid, Guid noteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cria uma nova anotação
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<NoteDetailsResponse> CreateAsync(Guid userUid, CreateNoteRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza uma anotação
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="noteId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<NoteDetailsResponse> UpdateAsync(Guid userUid, Guid noteId, UpdateNoteRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Exclui uma anotação
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="noteId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid userUid, Guid noteId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sincroniza dados
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<NoteSyncResponse> SyncAsync(Guid userUid, SyncNotesRequest request, CancellationToken cancellationToken = default);
    }
}
