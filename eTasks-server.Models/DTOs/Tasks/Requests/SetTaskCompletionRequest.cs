namespace eTasks_server.Models.DTOs.Tasks.Requests
{
    /// <summary>
    /// Dados para marcar ou desmarcar uma tarefa como concluida.
    /// </summary>
    public class SetTaskCompletionRequest
    {
        /// <summary>
        /// Indica se a tarefa deve ficar concluida.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
