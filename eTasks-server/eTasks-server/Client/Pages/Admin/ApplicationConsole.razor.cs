using eTasks_server.Core.Services.Interfaces;
using eTasks_server.Models.DTOs.ApplicationLogs.Responses;
using Microsoft.AspNetCore.Components;

namespace eTasks_server.Client.Pages.Admin
{
    public partial class ApplicationConsolePage : ComponentBase, IDisposable
    {
        [Inject] private IRealtimeLogStore RealtimeLogStore { get; set; } = default!;

        protected List<RealtimeLogEntryResponse> _entries = [];
        private bool _disposed;

        protected override void OnInitialized()
        {
            _entries = RealtimeLogStore.GetSnapshot().ToList();
            RealtimeLogStore.EntryAdded += OnEntryAdded;
        }

        protected void Clear()
        {
            RealtimeLogStore.Clear();
            _entries.Clear();
        }

        protected static string GetLevelClass(string level)
            => level.ToLowerInvariant() switch
            {
                "error" or "fatal" => "console-error",
                "warning" => "console-warning",
                "debug" or "verbose" => "console-muted",
                _ => "console-info"
            };

        private void OnEntryAdded(RealtimeLogEntryResponse entry)
        {
            if (_disposed)
            {
                return;
            }

            _ = InvokeAsync(() =>
            {
                _entries.Add(entry);
                if (_entries.Count > 500)
                {
                    _entries.RemoveRange(0, _entries.Count - 500);
                }

                StateHasChanged();
            });
        }

        public void Dispose()
        {
            _disposed = true;
            RealtimeLogStore.EntryAdded -= OnEntryAdded;
        }
    }
}
