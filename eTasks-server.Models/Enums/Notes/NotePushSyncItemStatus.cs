namespace eTasks_server.Models.Enums.Notes
{
    /// <summary>
    /// enumerado com status possíveis para o resultado do push de sincronização de uma nota
    /// </summary>
    public enum NotePushSyncItemStatus
    {
        /// <summary>
        /// Aplicado com sucesso
        /// </summary>
        Applied = 1,

        /// <summary>
        /// Ocorreu um conflito de sincronização, o item não foi aplicado
        /// </summary>
        Conflict = 2,

        /// <summary>
        /// Erro de validação, o item não foi aplicado
        /// </summary>
        ValidationError = 3,

        /// <summary>
        /// Não encontrado, o item não foi aplicado
        /// </summary>
        NotFound = 4,

        /// <summary>
        /// Falha inesperada, o item não foi aplicado
        /// </summary>
        Failed = 5
    }
}
