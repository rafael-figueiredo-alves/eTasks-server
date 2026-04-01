using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace eTasks_server.Core.Handlers
{
    public class CurrentRequestAuthHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentRequestAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is not null)
            {
                if (httpContext.Request.Headers.TryGetValue("Cookie", out var cookieHeader)
                    && !request.Headers.Contains("Cookie"))
                {
                    request.Headers.Add("Cookie", cookieHeader.ToString());
                }

                if (httpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader)
                    && AuthenticationHeaderValue.TryParse(authorizationHeader.ToString(), out var authValue)
                    && request.Headers.Authorization is null)
                {
                    request.Headers.Authorization = authValue;
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
