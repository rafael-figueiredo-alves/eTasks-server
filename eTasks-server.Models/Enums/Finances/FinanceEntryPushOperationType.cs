namespace eTasks_server.Models.Enums.Finances
{
    /// <summary>
    /// Tipo de operação para a entrada financeira (criação, atualização ou exclusão).
    /// </summary>
    public enum FinanceEntryPushOperationType
    {
        /// <summary>
        /// Criação de uma nova entrada financeira.
        /// </summary>
        Create = 0,

        /// <summary>
        /// Atualização de uma entrada financeira existente.
        /// </summary>
        Update = 1,

        /// <summary>
        /// Remover uma entrada financeira existente.
        /// </summary>
        Delete = 2
    }
}
