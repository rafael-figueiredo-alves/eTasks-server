using eTasks_server.Models.DTOs.Tasks.Requests;
using eTasks_server.Models.DTOs.Tasks.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Tasks
{
    /// <summary>
    /// Classe utilitaria para construir ETags das respostas de tarefas, garantindo que mudancas nos filtros,
    /// nos campos relevantes das tarefas ou nas configuracoes de recorrencia invalidem corretamente o cache HTTP.
    /// </summary>
    internal static class TaskEtagHelper
    {
        /// <summary>
        /// Cria um ETag para uma lista de tarefas com base nos filtros da requisicao e nos campos usados para
        /// representar cada item da colecao, incluindo dados relevantes de conclusao e recorrencia.
        /// </summary>
        /// <param name="tasks">Tarefas retornadas pela consulta.</param>
        /// <param name="request">Filtros aplicados na listagem.</param>
        /// <returns>ETag entre aspas para uso no header HTTP ETag.</returns>
        public static string BuildListEtag(IEnumerable<TaskListItemResponse> tasks, ListTasksRequest request)
        {
            var builder = new StringBuilder();
            builder.Append(request.ReferenceDate?.Date.ToString("yyyy-MM-dd") ?? string.Empty).Append('|');
            builder.Append(request.DateFrom?.Date.ToString("yyyy-MM-dd") ?? string.Empty).Append('|');
            builder.Append(request.DateTo?.Date.ToString("yyyy-MM-dd") ?? string.Empty).Append('|');
            builder.Append(request.IsCompleted?.ToString()).Append('|');
            builder.Append(request.Priority?.ToString()).Append('|');
            builder.Append(request.SearchTerm ?? string.Empty).Append('|');
            builder.Append(request.IncludeRecurring).Append('|');

            foreach (var task in tasks.OrderBy(x => x.TaskDate).ThenBy(x => x.Id))
            {
                builder.Append(task.Id).Append('|')
                    .Append(task.TaskDate.ToString("O")).Append('|')
                    .Append(task.IsCompleted).Append('|')
                    .Append(task.CompletedAt?.ToString("O")).Append('|')
                    .Append((int)task.Priority).Append('|')
                    .Append(task.Summary).Append('|')
                    .Append(task.HasRecurrence).Append(';');
            }

            return BuildQuotedHash(builder.ToString());
        }

        /// <summary>
        /// Cria um ETag para os detalhes de uma tarefa com base nos campos da tarefa e da recorrencia associada,
        /// quando existir.
        /// </summary>
        /// <param name="task">Tarefa detalhada.</param>
        /// <returns>ETag entre aspas para uso no header HTTP ETag.</returns>
        public static string BuildDetailsEtag(TaskDetailsResponse task)
        {
            var payload = string.Join("|",
                task.Id,
                task.UserUid,
                task.GeneratedFromTaskId,
                task.Summary,
                task.Notes,
                (int)task.Priority,
                task.TaskDate.ToString("O"),
                task.IsCompleted,
                task.CompletedAt?.ToString("O"),
                task.CreatedAt.ToString("O"),
                task.UpdatedAt?.ToString("O"),
                task.Recurrence?.Id,
                task.Recurrence?.RecurrenceType,
                task.Recurrence?.Interval,
                task.Recurrence?.WeekDays,
                task.Recurrence?.DayOfMonth,
                task.Recurrence?.MonthOfYear,
                task.Recurrence?.StartsOn.ToString("O") ?? string.Empty,
                task.Recurrence?.EndsOn?.ToString("O"),
                task.Recurrence?.LastGeneratedAt?.ToString("O"),
                task.Recurrence?.IsActive);

            return BuildQuotedHash(payload);
        }

        /// <summary>
        /// Constroi um ETag forte a partir do valor textual informado, usando SHA256 e formato hexadecimal.
        /// </summary>
        /// <param name="value">Conteudo canonico usado como base do hash.</param>
        /// <returns>Hash SHA256 formatado como ETag HTTP entre aspas.</returns>
        private static string BuildQuotedHash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
