using Microsoft.AspNetCore.Components;
using eTasks_server.Models.DTOs.OperationAudit.Responses;

namespace eTasks_server.Client.Components
{
    public class MetricRowBase : ComponentBase
    {
        [Parameter, EditorRequired] public OperationAuditMetricResponse Metric { get; set; } = default!;
        [Parameter] public long Total { get; set; }

        protected double PercentValue => Total <= 0 ? 0 : Math.Round(Metric.Count * 100d / Total, 2);
        protected string Percent => $"{PercentValue:0.##}%";
    }
}
