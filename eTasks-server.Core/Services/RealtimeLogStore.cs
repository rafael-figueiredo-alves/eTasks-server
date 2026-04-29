using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Core.Services
{
    public class RealtimeLogStore : IRealtimeLogStore
    {
        private const int MaxEntries = 500;
        private readonly Lock _lock = new();
        private readonly Queue<RealtimeLogEntryResponse> _entries = new();

        public event Action<RealtimeLogEntryResponse>? EntryAdded;

        public IReadOnlyList<RealtimeLogEntryResponse> GetSnapshot()
        {
            lock (_lock)
            {
                return _entries.ToList();
            }
        }

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

        public void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
            }
        }
    }
}
