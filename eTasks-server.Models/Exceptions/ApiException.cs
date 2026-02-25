using System.Net;

namespace eTasks_server.Models.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? Content { get; set; }

        public ApiException(HttpStatusCode statusCode, string? content = null, string message = "Erro ao consumir API") : base(message)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }
}
