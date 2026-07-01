namespace eTasks_server.Models.Enums.Goals
{
    /// <summary>
    /// Operações possíveis para sincronizar metas entre o cliente e o servidor.
    /// </summary>
    public enum GoalPushOperationType
    {
        /// <summary>
        /// Indica que a operação é para criar uma nova meta.
        /// </summary>
        Create = 1,

        /// <summary>
        /// Atualiza uma meta existente com novos dados.
        /// </summary>
        Update = 2,

        /// <summary>
        /// Remove uma meta existente do servidor.
        /// </summary>
        Delete = 3
    }
}
