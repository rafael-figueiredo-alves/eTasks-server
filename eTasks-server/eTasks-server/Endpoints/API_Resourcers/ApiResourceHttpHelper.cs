using eTasks_server.Models.Exceptions;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers
{
    internal static class ApiResourceHttpHelper
    {
        public static bool RequestMatchesIfNoneMatch(HttpRequest request, string currentEtag)
        {
            if (!request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var values))
            {
                return false;
            }

            return values.Any(value =>
            {
                var candidate = value?.Trim();
                return string.Equals(candidate, currentEtag, StringComparison.Ordinal) || candidate == "*";
            });
        }

        public static void EnsureIfMatch(HttpRequest request, string currentEtag, string conflictMessage)
        {
            if (!request.Headers.TryGetValue(HeaderNames.IfMatch, out var values) || values.Count == 0)
            {
                return;
            }

            var matched = values.Any(value =>
            {
                var candidate = value?.Trim();
                return string.Equals(candidate, currentEtag, StringComparison.Ordinal) || candidate == "*";
            });
            if (!matched)
            {
                throw new ApiException(HttpStatusCode.PreconditionFailed, conflictMessage);
            }
        }

        public static string FlattenValidationErrors(ValidationException exception)
        {
            return string.Join("; ", exception.Errors.SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}")));
        }
    }
}
