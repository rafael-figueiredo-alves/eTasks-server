using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace eTasks_server.Core.Services.Models
{
    /// <summary>
    /// Classe que representa um registro de auditoria de operação, contendo informações sobre a requisição HTTP, o usuário e o resultado da operação.
    /// </summary>
    public class OperationAuditLog
    {
        /// <summary>
        /// Obtém ou define o identificador único do registro de auditoria.
        /// </summary>
        [BsonId]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <summary>
        /// Obtém ou define a data e hora em que o registro de auditoria foi criado, em formato UTC.
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtém ou define o identificador de rastreamento da requisição HTTP, que pode ser usado para correlacionar logs e rastrear a execução da operação.
        /// </summary>
        public string TraceIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o método HTTP da requisição (por exemplo, GET, POST, PUT, DELETE).
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define o caminho da requisição HTTP, que representa a URL solicitada.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Obtém ou define a string de consulta (query string) da requisição HTTP, que contém parâmetros adicionais passados na URL.
        /// </summary>
        public string? QueryString { get; set; }

        /// <summary>
        /// Obtém ou define o nome do endpoint que foi chamado na requisição HTTP, se aplicável.
        /// </summary>
        public string? EndpointName { get; set; }

        /// <summary>
        /// Obtém ou define o nome do recurso que foi acessado na requisição HTTP, se aplicável.
        /// </summary>
        public string? ResourceName { get; set; }

        /// <summary>
        /// Obtém ou define o código de status HTTP retornado pela operação, indicando o resultado da requisição (por exemplo, 200 para sucesso, 404 para não encontrado, 500 para erro interno do servidor).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Obtém ou define a duração da operação em milissegundos, representando o tempo total gasto para processar a requisição HTTP.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Obtém ou define o identificador único do usuário que realizou a operação, se disponível. Caso o usuário não esteja autenticado, este valor será nulo.
        /// </summary>
        public Guid? UserUid { get; set; }

        /// <summary>
        /// Obtém ou define um valor booleano que indica se o usuário estava autenticado durante a operação. Se verdadeiro, significa que o usuário estava autenticado; caso contrário, significa que o usuário não estava autenticado.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Obtém ou define o agente do usuário (user agent) da requisição HTTP, que geralmente contém informações sobre o navegador, sistema operacional e dispositivo utilizado pelo cliente.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Obtém ou define o endereço IP do cliente que realizou a requisição HTTP, se disponível. Este valor pode ser útil para fins de auditoria e rastreamento de atividades suspeitas.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Obtém ou define a mensagem de erro retornada pela operação, se houver algum erro. Caso a operação tenha sido bem-sucedida, este valor será nulo.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
