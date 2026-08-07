using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Core.Services.Interfaces
{
    /// <summary>
    /// Interface para um serviço de armazenamento de logs em tempo real.
    /// </summary>
    public interface IRealtimeLogStore
    {
        /// <summary>
        /// Evento disparado quando uma nova entrada de log é adicionada ao armazenamento.
        /// </summary>
        event Action<RealtimeLogEntryResponse>? EntryAdded;

        /// <summary>
        /// Obtém uma lista somente leitura das entradas de log armazenadas atualmente.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<RealtimeLogEntryResponse> GetSnapshot();

        /// <summary>
        /// Adiciona uma nova entrada de log ao armazenamento e dispara o evento EntryAdded.
        /// </summary>
        /// <param name="entry">Entrada de log a ser adicionada</param>
        void Publish(RealtimeLogEntryResponse entry);

        /// <summary>
        /// Limpa todas as entradas de log armazenadas.
        /// </summary>
        void Clear();
    }
}
