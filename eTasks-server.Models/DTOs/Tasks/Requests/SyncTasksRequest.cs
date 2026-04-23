namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Parametros de sincronizacao incremental de tarefas.
    /// </summary>
    public class SyncTasksRequest
    {
        /// <summary>
        /// Cursor temporal da ultima sincronizacao bem sucedida no cliente.
        /// Quando nulo, retorna a carga inicial.
        /// </summary>
        public DateTime? Since { get; set; }

        /// <summary>
        /// Data inicial da janela para materializacao de recorrencias.
        /// </summary>
        public DateTime? WindowStart { get; set; }

        /// <summary>
        /// Data final da janela para materializacao de recorrencias.
        /// </summary>
        public DateTime? WindowEnd { get; set; }

        /// <summary>
        /// Indica se recorrencias devem ser materializadas na janela informada antes da sincronizacao.
        /// </summary>
        public bool IncludeRecurring { get; set; } = true;
    }
}
