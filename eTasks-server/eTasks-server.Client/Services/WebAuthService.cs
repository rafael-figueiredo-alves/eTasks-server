using eTasks_server.Client.Services.Interfaces;
using eTasks_server.Models.Auth;
using eTasks_server.Models.Exceptions;
using Microsoft.JSInterop;
using System.Net;

namespace eTasks_server.Client.Services
{
    public class WebAuthService : IWebAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public WebAuthService(
            HttpClient httpClient,
            IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task LoginAsync(WebLoginRequest request)
        {
            var endpoint = new Uri(_httpClient.BaseAddress!, "web-auth/login").ToString();
            var response = await _jsRuntime.InvokeAsync<WebAuthJsResponse>("webAuth.login", endpoint, request);
            EnsureSuccess(response);
        }

        public async Task LogoutAsync()
        {
            var endpoint = new Uri(_httpClient.BaseAddress!, "web-auth/logout").ToString();
            var response = await _jsRuntime.InvokeAsync<WebAuthJsResponse>("webAuth.logout", endpoint);
            EnsureSuccess(response);
        }

        private static void EnsureSuccess(WebAuthJsResponse response)
        {
            if (response.IsNetworkError)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, response.Body ?? "Erro de rede ao consumir autenticação web.");
            }

            if ((int)response.StatusCode is < 200 or >= 300)
            {
                throw new ApiException(response.StatusCode, response.Body, $"Erro ao consumir API: {response.StatusCode}");
            }
        }

        public sealed class WebAuthJsResponse
        {
            public bool IsNetworkError { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public string? Body { get; set; }
        }
    }
}
