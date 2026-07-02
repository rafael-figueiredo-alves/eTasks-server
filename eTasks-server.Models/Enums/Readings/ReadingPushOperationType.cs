namespace eTasks_server.Models.Enums.Readings
{
    /// <summary>
    /// Enumerado que representa as operações de push de leitura.
    /// </summary>
    public enum ReadingPushOperationType
    {
        /// <summary>
        /// Criar uma nova leitura.
        /// </summary>
        Create = 0,

        /// <summary>
        /// Atualizar uma leitura existente.
        /// </summary>
        Update = 1,

        /// <summary>
        /// Atualizar o progresso de uma leitura existente.
        /// </summary>
        UpdateProgress = 2,

        /// <summary>
        /// Excluir uma leitura existente.
        /// </summary>
        Delete = 3
    }
}
