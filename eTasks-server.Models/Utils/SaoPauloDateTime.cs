namespace eTasks_server.Models.Utils
{
    public static class SaoPauloDateTime
    {
        private static readonly Lazy<TimeZoneInfo> TimeZone = new(ResolveTimeZone);

        public static DateTime Now()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZone.Value);
        }

        public static DateTime ConvertFromUtc(DateTime utcDateTime)
        {
            var normalizedUtc = utcDateTime.Kind == DateTimeKind.Utc
                ? utcDateTime
                : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(normalizedUtc, TimeZone.Value);
        }

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
