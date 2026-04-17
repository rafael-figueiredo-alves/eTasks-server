using eTasks_server.Models.Entities.Finances;

namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Dados para atualizar um lancamento financeiro.
    /// </summary>
    public class UpdateFinanceEntryRequest
    {
        /// <summary>
        /// Titulo do lancamento financeiro. Ex: "Aluguel de Janeiro", "Salário de Fevereiro", etc.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Descrição da entrada financeira
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Categoria da entrada financeira
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Contraparte envolvida na transação financeira. Ex: "Supermercado XYZ", "Empresa ABC", etc.
        /// </summary>
        public string? Counterparty { get; set; }

        /// <summary>
        /// Tipo de lançamento financeiro, indicando se é uma receita (Credit) ou despesa (Debit).
        /// </summary>
        public FinanceEntryType EntryType { get; set; } = FinanceEntryType.Debit;

        /// <summary>
        /// Forma de pagamento da entrada financeira. Ex: "Cartão de Crédito", "Dinheiro", etc.
        /// </summary>
        public FinancePaymentMethod PaymentMethod { get; set; } = FinancePaymentMethod.Other;

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
        /// Quando a entrada financeira foi paga, caso já tenha sido. Ex: "2024-01-20", "2024-02-05", etc.
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Indica se a entrada financeira é recorrente ou não. Ex: true (recorrente), false (não recorrente).
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Dados sobre a recorrência da entrada financeira, caso seja recorrente. Ex: frequência (diária, semanal, mensal, etc.), data de término da recorrência, etc.
        /// </summary>
        public FinanceRecurrenceRequest? Recurrence { get; set; }
    }
}
