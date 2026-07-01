namespace eTasks_server.Models.Enums.Goals
{
    /// <summary>
    /// Status da operação de sincronização da meta (GoalPushSyncItem).
    /// </summary>
    public enum GoalPushSyncItemStatus
    {
        /// <summary>
        /// A operação foi bem-sucedida e a meta foi sincronizada com sucesso.
        /// </summary>
        Applied = 1,

        /// <summary>
        /// A operação falhou devido a um conflito de dados, como uma versão desatualizada da meta ou uma tentativa de atualizar uma meta que foi excluída.
        /// </summary>
        Conflict = 2,
        
        /// <summary>
        /// A operação falhou devido a uma validação de dados.
        /// </summary>
        ValidationError = 3,

        /// <summary>
        /// Não foi possível encontrar a meta para sincronização, indicando que ela pode ter sido excluída ou nunca existiu.
        /// </summary>
        NotFound = 4,

        /// <summary>
        /// A operação falhou por um motivo não especificado.
        /// </summary>
        Failed = 5
    }
}
