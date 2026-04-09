namespace eTasks_server.Models.Entities.Finances
{
    /// <summary>
    /// Define a forma de pagamento usada no lançamento financeiro.
    /// </summary>
    public enum FinancePaymentMethod
    {
        /// <summary>
        /// Outro meio de pagamento não categorizado.
        /// </summary>
        Other = 0,
        /// <summary>
        /// Pagamento em dinheiro.
        /// </summary>
        Cash = 1,
        /// <summary>
        /// Pagamento via PIX.
        /// </summary>
        Pix = 2,
        /// <summary>
        /// Pagamento com cartão de débito.
        /// </summary>
        DebitCard = 3,
        /// <summary>
        /// Pagamento com cartão de crédito.
        /// </summary>
        CreditCard = 4,
        /// <summary>
        /// Transferência bancária.
        /// </summary>
        BankTransfer = 5,
        /// <summary>
        /// Pagamento por boleto.
        /// </summary>
        BankSlip = 6
    }
}
