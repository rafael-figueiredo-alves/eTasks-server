namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Enum sobre operaçoes de push de notas, representando as ações possíveis: criar, atualizar e deletar.
    /// </summary>
    public enum NotePushOperationType
    {
        /// <summary>
        /// Representa a operação de criação de uma nota.
        /// </summary>
        Create = 1,

        /// <summary>
        /// Representa a operação de atualização de uma nota.
        /// </summary>
        Update = 2,

        /// <summary>
        /// Representa a operação de exclusão de uma nota.
        /// </summary>
        Delete = 3
    }
}
