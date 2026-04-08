namespace eTasks_server.Models.Utils
{
    /// <summary>
    /// Classe para converter automaticamente 
    /// </summary>
    public static class SaoPauloDateTime
    {
        private static readonly Lazy<TimeZoneInfo> TimeZone = new(ResolveTimeZone);

        /// <summary>
        /// Pega hora e data atual convertida para o fuso horário de São Paulo
        /// </summary>
        /// <returns></returns>
        public static DateTime Now()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZone.Value);
        }

        /// <summary>
        /// Converte um valor de data e hora no formato UTC para o fuso horário configurado na aplicação.
        /// </summary>
        /// <remarks>Se o parâmetro especificado não estiver marcado como UTC, ele será tratado como se
        /// estivesse em UTC. O fuso horário de destino é determinado pela propriedade TimeZone da aplicação.</remarks>
        /// <param name="utcDateTime">A data e hora em UTC a ser convertida. O valor pode ter qualquer especificação de Kind, mas será tratado
        /// como UTC.</param>
        /// <returns>Um valor DateTime representando a data e hora correspondente no fuso horário configurado.</returns>
        public static DateTime ConvertFromUtc(DateTime utcDateTime)
        {
            var normalizedUtc = utcDateTime.Kind == DateTimeKind.Utc
                ? utcDateTime
                : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(normalizedUtc, TimeZone.Value);
        }

        /// <summary>
        /// Ajusta o horário de um valor DateTime para o fuso horário configurado na aplicação, considerando o horário de verão, se aplicável.
        /// </summary>
        /// <returns></returns>
        private static TimeZoneInfo ResolveTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
        }
    }
}
