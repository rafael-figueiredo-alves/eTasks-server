using eTasks_server.Models.Entities.Finances;

namespace eTasks_server.Models.DTOs.Finances.Requests
{
    /// <summary>
    /// Filtros de consulta para financas.
    /// </summary>
    public class ListFinanceEntriesRequest
    {
        /// <summary>
        /// Ano para filtrar as entradas financeiras. Se fornecido, retornará apenas as entradas do ano especificado.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Mês para filtrar as entradas financeiras. Se fornecido, retornará apenas as entradas do mês especificado. Deve ser usado em conjunto com o filtro de ano para garantir resultados precisos.
        /// </summary>
        public int? Month { get; set; }
        
        /// <summary>
        /// Data inicial para filtrar as entradas financeiras. Se fornecido, retornará apenas as entradas a partir desta data.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Data final para filtrar as entradas financeiras. Se fornecido, retornará apenas as entradas até esta data.
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Tipo de entrada financeira para filtrar os resultados. Se fornecido, retornará apenas as entradas do tipo especificado (por exemplo, receita ou despesa).
        /// </summary>
        public FinanceEntryType? EntryType { get; set; }

        /// <summary>
        /// Forma de pagamento para filtrar as entradas financeiras. Se fornecido, retornará apenas as entradas associadas à forma de pagamento especificada (por exemplo, cartão de crédito, dinheiro, etc.).
        /// </summary>
        public FinancePaymentMethod? PaymentMethod { get; set; }
        
        /// <summary>
        /// Indica se a entrada financeira foi paga. Se fornecido, retornará apenas as entradas que correspondem ao status de pagamento especificado.
        /// </summary>
        public bool? IsPaid { get; set; }

        /// <summary>
        /// Indica se a entrada financeira é recorrente. Se fornecido, retornará apenas as entradas que correspondem ao status de recorrência especificado.
        /// </summary>
        public bool? IsRecurring { get; set; }

        /// <summary>
        /// Categoria da entrada financeira para filtrar os resultados. Se fornecido, retornará apenas as entradas da categoria especificada.
        /// </summary>
        public string? Category { get; set; }
        
        /// <summary>
        /// Termo de pesquisa para filtrar as entradas financeiras. Se fornecido, retornará apenas as entradas que correspondem ao termo de pesquisa especificado.
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
