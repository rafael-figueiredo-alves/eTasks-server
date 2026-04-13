using eTasks_server.Models.Entities.Finances;

namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Dados para criar um lancamento financeiro.
    /// </summary>
    public class CreateFinanceEntryRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Counterparty { get; set; }
        public FinanceEntryType EntryType { get; set; } = FinanceEntryType.Debit;
        public FinancePaymentMethod PaymentMethod { get; set; } = FinancePaymentMethod.Other;
        public decimal Amount { get; set; }
        public DateTime OccursOn { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public bool IsRecurring { get; set; }
        public FinanceRecurrenceRequest? Recurrence { get; set; }
    }
}
