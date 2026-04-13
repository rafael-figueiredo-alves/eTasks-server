namespace eTasks_server.Models.DTOs.Finances.Responses
{
    /// <summary>
    /// Resumo financeiro mensal.
    /// </summary>
    public class FinanceMonthSummaryResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal Balance { get; set; }
        public bool IsPositiveBalance { get; set; }
        public bool EligibleForBonusPoints { get; set; }
    }
}
