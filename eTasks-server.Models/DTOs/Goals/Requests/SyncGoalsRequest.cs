namespace eTasks_server.Models.DTOs.Goals.Requests
{
    /// <summary>
    /// Dados de entrada para sincronizar metas.
    /// </summary>
    public class SyncGoalsRequest
    {
        /// <summary>
        /// Data a partir da qual as metas devem ser sincronizadas. Se não for fornecida, todas as metas serão sincronizadas.
        /// </summary>
        public DateTime? Since { get; set; }
    }
}
