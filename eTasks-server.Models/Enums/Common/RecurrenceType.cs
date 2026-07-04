namespace eTasks_server.Models.Enums.Common
{
    /// <summary>
    /// Representa os tipos básicos de recorrência suportados.
    /// </summary>
    public enum RecurrenceType
    {
        /// <summary>
        /// Sem recorrência.
        /// </summary>
        None = 0,
        /// <summary>
        /// Recorrência diária.
        /// </summary>
        Daily = 1,
        /// <summary>
        /// Recorrência semanal.
        /// </summary>
        Weekly = 2,
        /// <summary>
        /// Recorrência mensal.
        /// </summary>
        Monthly = 3,
        /// <summary>
        /// Recorrência anual.
        /// </summary>
        Yearly = 4
    }
}
