using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ApplicationLogs.Responses;
using Serilog.Core;
using Serilog.Events;

namespace eTasks_server.Extensions
{
    public class RealtimeLogSink(IRealtimeLogStore store) : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            store.Publish(new RealtimeLogEntryResponse
            {
                Timestamp = logEvent.Timestamp.LocalDateTime,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                Exception = logEvent.Exception?.ToString(),
                Source = logEvent.Properties.TryGetValue("SourceContext", out var source)
                    ? source.ToString().Trim('"')
                    : string.Empty
            });
        }
    }
}
