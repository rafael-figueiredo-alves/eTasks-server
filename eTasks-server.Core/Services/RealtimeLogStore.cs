using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Core.Services
{
    /// <summary>
    /// Serviço de log real-time que armazena entradas de log em memória e permite a publicação e assinatura de eventos de log.
    /// </summary>
    public class RealtimeLogStore : IRealtimeLogStore
    {
        private const int MaxEntries = 500;
        private readonly Lock _lock = new();
        private readonly Queue<RealtimeLogEntryResponse> _entries = new();

        public event Action<RealtimeLogEntryResponse>? EntryAdded;

        /// <summary>
        /// Obtém uma cópia das entradas de log armazenadas atualmente.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<RealtimeLogEntryResponse> GetSnapshot()
        {
            lock (_lock)
            {
                return _entries.ToList();
            }
        }

        /// <summary>
        /// Publica uma nova entrada de log no serviço, adicionando-a à fila e acionando o evento EntryAdded.
        /// </summary>
        /// <param name="entry"></param>
        public void Publish(RealtimeLogEntryResponse entry)
        {
            lock (_lock)
            {
                _entries.Enqueue(entry);
                while (_entries.Count > MaxEntries)
                {
                    _entries.Dequeue();
                }
            }

            EntryAdded?.Invoke(entry);
        }

        /// <summary>
        /// Limpa todas as entradas de log armazenadas no serviço.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
            }
        }
    }
}
