namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Resumo financeiro mensal.
    /// </summary>
    public class FinanceMonthSummaryResponse
    {
        /// <summary>
        /// Ano do resumo financeiro.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Mês do resumo financeiro (1-12).
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Total de créditos (entradas) no mês.
        /// </summary>
        public decimal TotalCredits { get; set; }

        /// <summary>
        /// Total de débitos (saídas) no mês.
        /// </summary>
        public decimal TotalDebits { get; set; }

        /// <summary>
        /// Balance líquido do mês (TotalCredits - TotalDebits).
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Indica se o balance é positivo (true) ou negativo (false).
        /// </summary>
        public bool IsPositiveBalance { get; set; }

        /// <summary>
        /// Indica se é elegível para pontos de bônus (true) ou não (false).
        /// </summary>
        public bool EligibleForBonusPoints { get; set; }
    }
}
