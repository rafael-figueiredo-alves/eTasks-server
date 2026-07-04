namespace eTasks_server.Models.Enums.Finances
{
    /// <summary>
    /// Define se o lançamento financeiro é crédito ou débito.
    /// </summary>
    public enum FinanceEntryType
    {
        /// <summary>
        /// Entrada de valor.
        /// </summary>
        Credit = 0,
        /// <summary>
        /// Saída de valor.
        /// </summary>
        Debit = 1
    }
}
