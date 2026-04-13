using eTasks_server.Models.Entities.Finances;

namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Filtros de consulta para financas.
    /// </summary>
    public class ListFinanceEntriesRequest
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public FinanceEntryType? EntryType { get; set; }
        public FinancePaymentMethod? PaymentMethod { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsRecurring { get; set; }
        public string? Category { get; set; }
        public string? SearchTerm { get; set; }
    }
}
