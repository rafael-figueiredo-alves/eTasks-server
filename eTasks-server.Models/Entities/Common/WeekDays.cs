using System;

namespace eTasks_server.Models.Entities.Common
{
    /// <summary>
    /// Representa os dias da semana com suporte a combinação por flags.
    /// </summary>
    [Flags]
    public enum WeekDays
    {
        /// <summary>
        /// Nenhum dia selecionado.
        /// </summary>
        None = 0,
        /// <summary>
        /// Domingo.
        /// </summary>
        Sunday = 1,
        /// <summary>
        /// Segunda-feira.
        /// </summary>
        Monday = 2,
        /// <summary>
        /// Terça-feira.
        /// </summary>
        Tuesday = 4,
        /// <summary>
        /// Quarta-feira.
        /// </summary>
        Wednesday = 8,
        /// <summary>
        /// Quinta-feira.
        /// </summary>
        Thursday = 16,
        /// <summary>
        /// Sexta-feira.
        /// </summary>
        Friday = 32,
        /// <summary>
        /// Sábado.
        /// </summary>
        Saturday = 64
    }
}
