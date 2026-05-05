using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ApplicationLogs.Responses;
using Serilog.Core;
using Serilog.Events;

namespace eTasks_server.Extensions
{
    /// <summary>
    /// Esta é uma classe que auxilia na criação de um sink personalizado para o Serilog, permitindo que os eventos de log sejam enviados em tempo real para um armazenamento específico. O `RealtimeLogSink` implementa a interface `ILogEventSink`, o que significa que ele pode ser integrado ao pipeline de logging do Serilog para processar e encaminhar os eventos de log conforme são gerados.
    /// </summary>
    /// <remarks>This sink is intended for scenarios where log events need to be streamed or processed in real
    /// time, such as live dashboards or monitoring tools. The sink immediately forwards each log event to the specified
    /// store upon emission.</remarks>
    /// <param name="store">The real-time log store used to publish log entries. Cannot be null.</param>
    public class RealtimeLogSink(IRealtimeLogStore store) : ILogEventSink
    {
        /// <summary>
        /// Método responsável por processar cada evento de log recebido e publicá-lo no armazenamento em tempo real. Ele extrai as informações relevantes do evento de log, como o timestamp, nível, mensagem, exceção (se houver) e a fonte do log, e as encapsula em um objeto `RealtimeLogEntryResponse` antes de enviá-lo para o store. Este método é chamado automaticamente pelo Serilog sempre que um evento de log é emitido.
        /// </summary>
        /// <param name="logEvent">O evento de log a ser processado.</param>
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
