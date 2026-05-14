using eTasks_server.Models.Exceptions;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace eTasks_server.Endpoints.API_Resourcers
{
    /// <summary>
    /// Classe de utilidades para lidar com cabeçalhos HTTP relacionados a recursos de API, como ETags e validação de pré-condições.
    /// </summary>
    internal static class ApiResourceHttpHelper
    {
        /// <summary>
        /// Verifica se o cabeçalho "If-None-Match" da requisição corresponde ao ETag atual do recurso, indicando que o recurso não foi modificado desde a última vez que foi acessado.
        /// </summary>
        /// <param name="request">A requisição HTTP que contém os cabeçalhos a serem verificados.</param>
        /// <param name="currentEtag">O ETag atual do recurso.</param>
        /// <returns>Retorna true se o cabeçalho "If-None-Match" corresponder ao ETag atual, caso contrário, false.</returns>
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

        /// <summary>
        /// Método para garantir que o cabeçalho "If-Match" da requisição corresponda ao ETag atual do recurso, indicando que o recurso foi modificado desde a última vez que foi acessado. Se o cabeçalho "If-Match" não corresponder ao ETag atual, uma exceção ApiException é lançada com o status HTTP 412 Precondition Failed e uma mensagem de conflito personalizada.
        /// </summary>
        /// <param name="request">A requisição HTTP que contém os cabeçalhos a serem verificados.</param>
        /// <param name="currentEtag">O ETag atual do recurso.</param>
        /// <param name="conflictMessage">A mensagem de conflito a ser retornada em caso de falha na pré-condição.</param>
        /// <exception cref="ApiException">Lançada quando a pré-condição falha.</exception>
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

        /// <summary>
        /// Método para aplanar os erros de validação contidos em uma ValidationException em uma única string, formatando cada erro como "campo: mensagem de erro" e separando-os por ponto e vírgula.
        /// </summary>
        /// <param name="exception">A exceção de validação que contém os erros a serem aplanados.</param>
        /// <returns>Uma string contendo todos os erros de validação formatados.</returns>
        public static string FlattenValidationErrors(ValidationException exception)
        {
            return string.Join("; ", exception.Errors.SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}")));
        }
    }
}
