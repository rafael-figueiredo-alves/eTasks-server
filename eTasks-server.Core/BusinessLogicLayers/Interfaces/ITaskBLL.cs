using eTasks_server.Models.DTOs.Tasks.Requests;
using eTasks_server.Models.DTOs.Tasks.Responses;

namespace eTasks_server.Core.BusinessLogicLayers.Interfaces
{
    /// <summary>
    /// Regras de negocio para gerenciamento de tarefas do usuario autenticado.
    /// </summary>
    public interface ITaskBLL
    {
        /// <summary>
        /// Lista tarefas do usuario autenticado conforme os filtros informados.
        /// </summary>
        Task<List<TaskListItemResponse>> ListAsync(Guid userUid, ListTasksRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtem os detalhes de uma tarefa especifica.
        /// </summary>
        Task<TaskDetailsResponse> GetByIdAsync(Guid userUid, Guid taskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cria uma nova tarefa para o usuario autenticado.
        /// </summary>
        Task<TaskDetailsResponse> CreateAsync(Guid userUid, CreateTaskRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Atualiza uma tarefa pertencente ao usuario autenticado.
        /// </summary>
        Task<TaskDetailsResponse> UpdateAsync(Guid userUid, Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marca ou desmarca uma tarefa como concluida.
        /// </summary>
        Task<TaskDetailsResponse> SetCompletionAsync(Guid userUid, Guid taskId, bool isCompleted, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove logicamente uma tarefa do usuario autenticado.
        /// Quando a tarefa for recorrente base, remove tambem as ocorrencias geradas.
        /// </summary>
        Task DeleteAsync(Guid userUid, Guid taskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retorna alteracoes incrementais de tarefas para sincronizacao offline-first.
        /// </summary>
        Task<TaskSyncResponse> SyncAsync(Guid userUid, SyncTasksRequest request, CancellationToken cancellationToken = default);
    }
}
