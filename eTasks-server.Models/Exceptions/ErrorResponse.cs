using System.Text.Json.Serialization;

/// <summary>
/// Namespace que contém a classe ErrorResponse, utilizada para representar a estrutura de resposta de erro em casos de exceções na aplicação.
/// </summary>
namespace eTasks_server.Models.Exceptions
{
    /// <summary>
    /// Resposta de erro que será retornada em casos de exceções na aplicação, contendo informações como o ID do rastreamento, mensagem de erro e detalhes adicionais.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Identificador do rastreamento (traceId) que pode ser utilizado para correlacionar logs e identificar a origem do erro na aplicação.
        /// </summary>
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem de erro que descreve o motivo da exceção ou o problema ocorrido, fornecendo informações úteis para o diagnóstico e resolução do erro.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detalhes adicionais sobre o erro, que podem incluir informações técnicas, stack trace ou qualquer outra informação relevante para entender melhor a natureza do erro e facilitar a resolução do problema.
        /// </summary>
        [JsonPropertyName("details")]
        public string Details { get; set; } = string.Empty;
    }
}
