namespace eTasks_server.Models.DTOs.Notes.Requests
{
    /// <summary>
    /// Representa uma requisição para sincronizar notas, contendo a data e hora desde a qual as notas devem ser sincronizadas.
    /// </summary>
    public class SyncNotesRequest
    {
        /// <summary>
        /// Obtém ou define a data e hora desde a qual as notas devem ser sincronizadas. Se for nulo, todas as notas serão sincronizadas.
        /// </summary>
        public DateTime? Since { get; set; }
    }
}
