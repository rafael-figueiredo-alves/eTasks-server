using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace eTasks_server.Components.Pages
{
    public class ErrorBase : ComponentBase
    {
        [CascadingParameter]
        protected HttpContext? HttpContext { get; set; }

        protected string? RequestId { get; set; }
        protected bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        protected override void OnInitialized() =>
            RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
    }
}
