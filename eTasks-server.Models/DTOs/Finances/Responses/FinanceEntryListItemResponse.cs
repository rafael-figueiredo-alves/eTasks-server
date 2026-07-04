using eTasks_server.Models.Enums.Finances;

namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Item resumido da listagem de financas.
    /// </summary>
    public class FinanceEntryListItemResponse
    {
        /// <summary>
        /// Identificador único da entrada financeira.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Título ou descrição breve da entrada financeira.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Descrição detalhada da entrada financeira (opcional).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Categoria ou tipo da entrada financeira (opcional).
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Contraparte envolvida na transação financeira (opcional).
        /// </summary>
        public string? Counterparty { get; set; }

        /// <summary>
        /// Tipo de entrada financeira, indicando se é uma receita ou despesa.
        /// </summary>
        public FinanceEntryType EntryType { get; set; }

        /// <summary>
        /// Forma de pagamento
        /// </summary>
        public FinancePaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Valor monetário da entrada financeira, representando o mont
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Quando a entrada financeira ocorreu ou está programada para ocorrer, representando
        /// </summary>
        public DateTime OccursOn { get; set; }

        /// <summary>
        /// Indica se a entrada financeira já foi paga ou não, permitindo
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Data e hora em que a entrada financeira foi paga, se aplicável, permitindo
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Indicado se a entrada financeira é recorrente, permitindo identificar se ela se repete em intervalos regulares (por exemplo, mensalmente, anualmente) ou se é uma transação única.
        /// </summary>
        public bool IsRecurring { get; set; }
    }
}
