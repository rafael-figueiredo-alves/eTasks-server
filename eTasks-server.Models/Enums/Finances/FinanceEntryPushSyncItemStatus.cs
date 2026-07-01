namespace eTasks_server.Models.Enums.Finances
{
    /// <summary>
    /// Status da sincronização de um item de lançamento financeiro.
    /// </summary>
    public enum FinanceEntryPushSyncItemStatus
    {
        /// <summary>
        /// Aplicado com sucesso.
        /// </summary>
        Applied = 0,

        /// <summary>
        /// Conflito de dados, o item não pôde ser aplicado devido a um conflito com os dados existentes.
        /// </summary>
        Conflict = 1,

        /// <summary>
        /// Erro de validação, o item não pôde ser aplicado devido a erros de validação.
        /// </summary>
        ValidationError = 2,

        /// <summary>
        /// Não encontrado, o item não pôde ser aplicado porque não foi encontrado no sistema.
        /// </summary>
        NotFound = 3,

        /// <summary>
        /// Falha, o item não pôde ser aplicado devido a uma falha inesperada.
        /// </summary>
        Failed = 4
    }
}
