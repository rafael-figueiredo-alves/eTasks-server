namespace eTasks_server.Models.Enums.Shopping
{
    /// <summary>
    /// Enumerado do status de sincronização dos itens da lista de compras
    /// </summary>
    public enum ShoppingListPushSyncItemStatus
    {
        /// <summary>
        /// Aplicadas com sucesso
        /// </summary>
        Applied = 0,

        /// <summary>
        /// Houve conflito
        /// </summary>
        Conflict = 1,

        /// <summary>
        /// Ocorreu erro de validação
        /// </summary>
        ValidationError = 2,

        /// <summary>
        /// Não encontrado
        /// </summary>
        NotFound = 3,

        /// <summary>
        /// Falhou
        /// </summary>
        Failed = 4
    }
}
