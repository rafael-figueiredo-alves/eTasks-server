using eTasks_server.Models.DTOs.Finances.Requests;
using eTasks_server.Models.DTOs.Finances.Responses;
using System.Security.Cryptography;
using System.Text;

namespace eTasks_server.Endpoints.API_Resourcers.Finances
{
    internal static class FinanceEtagHelper
    {
        public static string BuildListEtag(IEnumerable<FinanceEntryListItemResponse> entries, ListFinanceEntriesRequest request)
        {
            var builder = new StringBuilder();
            builder.Append(request.Year?.ToString() ?? string.Empty).Append('|')
                .Append(request.Month?.ToString() ?? string.Empty).Append('|')
                .Append(request.DateFrom?.Ticks ?? 0).Append('|')
                .Append(request.DateTo?.Ticks ?? 0).Append('|')
                .Append(request.EntryType?.ToString() ?? string.Empty).Append('|')
                .Append(request.PaymentMethod?.ToString() ?? string.Empty).Append('|')
                .Append(request.IsPaid?.ToString() ?? string.Empty).Append('|')
                .Append(request.IsRecurring?.ToString() ?? string.Empty).Append('|')
                .Append(request.Category ?? string.Empty).Append('|')
                .Append(request.SearchTerm ?? string.Empty);

            foreach (var entry in entries.OrderByDescending(x => x.OccursOn).ThenBy(x => x.Id))
            {
                builder.Append('|')
                    .Append(entry.Id)
                    .Append('|').Append(entry.Title)
                    .Append('|').Append(entry.Amount)
                    .Append('|').Append(entry.OccursOn.Ticks)
                    .Append('|').Append(entry.IsPaid);
            }

            return BuildHash(builder.ToString());
        }

        public static string BuildDetailsEtag(FinanceEntryDetailsResponse entry)
        {
            var payload = string.Join('|',
                entry.Id,
                entry.UserUid,
                entry.Title,
                entry.Description ?? string.Empty,
                entry.Category ?? string.Empty,
                entry.Counterparty ?? string.Empty,
                entry.EntryType,
                entry.PaymentMethod,
                entry.Amount,
                entry.OccursOn.Ticks,
                entry.IsPaid,
                entry.PaidAt?.Ticks ?? 0,
                entry.IsRecurring,
                entry.Recurrence?.RecurrenceType,
                entry.Recurrence?.RecurrenceInterval,
                entry.Recurrence?.WeekDays,
                entry.Recurrence?.DayOfMonth,
                entry.Recurrence?.RecurrenceEndsOn?.Ticks ?? 0,
                entry.CreatedAt.Ticks,
                entry.UpdatedAt?.Ticks ?? 0);

            return BuildHash(payload);
        }

        private static string BuildHash(string payload)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
            return $"\"{Convert.ToHexString(bytes)}\"";
        }
    }
}
