using eTasks_server.Models.Entities.Finances;

namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Item resumido da listagem de financas.
    /// </summary>
    public class FinanceEntryListItemResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Counterparty { get; set; }
        public FinanceEntryType EntryType { get; set; }
        public FinancePaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public DateTime OccursOn { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public bool IsRecurring { get; set; }
    }
}
