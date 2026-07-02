namespace eTasks_server.Models.Enums.Readings
{
    /// <summary>
    /// Enumerado que representa os estados possíveis de um item de sincronização de leitura.
    /// </summary>
    public enum ReadingPushSyncItemStatus
    {
        /// <summary>
        /// Aplicado com sucesso.
        /// </summary>
        Applied = 0,

        /// <summary>
        /// Conflito detectado durante a sincronização.
        /// </summary>
        Conflict = 1,

        /// <summary>
        /// Erro de validação durante a sincronização.
        /// </summary>
        ValidationError = 2,

        /// <summary>
        /// Não encontrado durante a sincronização.
        /// </summary>
        NotFound = 3,

        /// <summary>
        /// Falha na sincronização devido a um erro inesperado.
        /// </summary>
        Failed = 4
    }
}
