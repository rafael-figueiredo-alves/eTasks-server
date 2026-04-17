using eTasks_server.Models.Entities.Finances;

namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Resposta detalhada de um lancamento financeiro.
    /// </summary>
    public class FinanceEntryDetailsResponse
    {
        /// <summary>
        /// Identificador único do lançamento financeiro.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador único do usuário associado ao lançamento financeiro.
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Título do lançamento financeiro, representando a descrição principal do mesmo.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do lançamento financeiro.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Categoria do lançamento financeiro.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Contraparte envolvida na transação financeira.
        /// </summary>
        public string? Counterparty { get; set; }

        /// <summary>
        /// Tipo de lançamento financeiro, indicando se é uma receita (Credit) ou despesa (Debit).
        /// </summary>
        public FinanceEntryType EntryType { get; set; }

        /// <summary>
        /// Forma de pagamento da entrada financeira. Ex: "Cartão de Crédito", "Dinheiro", etc.
        /// </summary>
        public FinancePaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Valor monetário da entrada financeira. Ex: 1500.00, 250.75, etc.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Data em que a entrada financeira ocorre ou ocorreu. Ex: "2024-01-15", "2024-02-28", etc.
        /// </summary>
        public DateTime OccursOn { get; set; }

        /// <summary>
        /// Indica se a entrada financeira já foi paga ou não. Ex: true (pago), false (não pago).
        /// </summary>
        public bool IsPaid { get; set; }
        
        /// <summary>
        /// Data em que a entrada financeira foi paga, caso já tenha sido. Ex: "2024-01-20", "2024-02-05", etc.
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Indica se é recorrente
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Traz dados da recorrência da entrada financeira
        /// </summary>
        public FinanceRecurrenceResponse? Recurrence { get; set; }

        /// <summary>
        /// Data da criação do registro da entrada financeira. Ex: "2024-01-10T14:30:00Z", "2024-02-01T09:15:00Z", etc.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data em que a entrada financeira foi atualizada pela última vez, caso tenha sido. Ex: "2024-01-12T16:45:00Z", "2024-02-03T11:20:00Z", etc.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
