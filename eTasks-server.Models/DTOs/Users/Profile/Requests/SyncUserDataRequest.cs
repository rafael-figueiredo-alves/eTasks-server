namespace eTasks_server.Models.DTOs.Users.Profile.Requests
{
    /// <summary>
    /// Requisição de sincronização de dados do usuário
    /// </summary>
    public class SyncUserDataRequest
    {
        /// <summary>
        /// Desde que data/hora para sincronizar
        /// </summary>
        public DateTime? Since { get; set; }
    }
}
