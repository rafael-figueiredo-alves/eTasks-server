using eTasks_server.Models.DTOs.ApplicationLogs.Responses;

namespace eTasks_server.Core.Services.Interfaces
{
    public interface IRealtimeLogStore
    {
        event Action<RealtimeLogEntryResponse>? EntryAdded;
        IReadOnlyList<RealtimeLogEntryResponse> GetSnapshot();
        void Publish(RealtimeLogEntryResponse entry);
        void Clear();
    }
}
