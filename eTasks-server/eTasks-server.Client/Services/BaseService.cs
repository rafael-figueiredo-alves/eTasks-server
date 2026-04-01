using eTasks_server.Models.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace eTasks_server.Client.Services
{
    public abstract class BaseService
    {
        private readonly HttpClient _httpClient;
        private readonly IDialogService _dialogService;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        protected BaseService(
            HttpClient httpClient,
            IDialogService dialogService,
            NavigationManager navigationManager,
            IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _dialogService = dialogService;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        protected async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }

            await HandleAuthorizationFailureAsync(response);

            var content = await response.Content.ReadAsStringAsync();
            throw new ApiException(response.StatusCode, content, $"Erro ao consumir API: {response.ReasonPhrase}");
        }

        protected async Task HandleResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await HandleAuthorizationFailureAsync(response);
                var content = await response.Content.ReadAsStringAsync();
                throw new ApiException(response.StatusCode, content, $"Erro ao consumir API: {response.ReasonPhrase}");
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponseAsync<T>(response);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            var response = await SendBrowserJsonAsync(HttpMethod.Post, endpoint, data);
            return await HandleResponseAsync<T>(response);
        }

        public async Task<bool> PostAsync(string endpoint, object data)
        {
            var response = await SendBrowserJsonAsync(HttpMethod.Post, endpoint, data);
            await HandleResponseAsync(response);
            return true;
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            var response = await SendBrowserJsonAsync(HttpMethod.Put, endpoint, data);
            return await HandleResponseAsync<T>(response);
        }

        public async Task<bool> PutAsync(string endpoint, object data)
        {
            var response = await SendBrowserJsonAsync(HttpMethod.Put, endpoint, data);
            await HandleResponseAsync(response);
            return true;
        }

        public async Task<T?> PatchAsync<T>(string endpoint, object data)
        {
            var response = await SendBrowserJsonAsync(HttpMethod.Patch, endpoint, data);
            return await HandleResponseAsync<T>(response);
        }

        public async Task<bool> PatchAsync(string endpoint, object data)
        {
            var response = await SendBrowserJsonAsync(HttpMethod.Patch, endpoint, data);
            await HandleResponseAsync(response);
            return true;
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            var response = await SendBrowserJsonAsync(HttpMethod.Delete, endpoint, null);
            await HandleResponseAsync(response);
            return true;
        }

        public async Task<HttpResponseMessage> OptionsAsync(string endpoint)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Options, endpoint);
                return await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }
        }

        private async Task HandleAuthorizationFailureAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var currentPath = "/" + _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
                if (string.IsNullOrWhiteSpace(currentPath) || currentPath == "/")
                {
                    currentPath = "/";
                }

                var encodedReturnUrl = Uri.EscapeDataString(currentPath);
                _navigationManager.NavigateTo($"/login?returnUrl={encodedReturnUrl}", forceLoad: true);
                return;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                await _dialogService.ShowMessageBoxAsync(
                    "Acesso negado",
                    "Você não tem permissão para acessar este recurso.",
                    yesText: "OK");
            }
        }

        private async Task<HttpResponseMessage> SendBrowserJsonAsync(HttpMethod method, string endpoint, object? data)
        {
            var url = new Uri(_httpClient.BaseAddress!, endpoint).ToString();
            var payload = data is null ? null : JsonSerializer.Serialize(data);

            BrowserHttpResponse response;
            try
            {
                response = await _jsRuntime.InvokeAsync<BrowserHttpResponse>(
                    "webAuth.send",
                    method.Method,
                    url,
                    payload);
            }
            catch (JSException ex)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, $"Erro de Rede: {ex.Message}");
            }

            if (response.IsNetworkError)
            {
                throw new ApiException(HttpStatusCode.ServiceUnavailable, null, response.Body ?? "Erro de rede ao consumir API.");
            }

            return new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(response.Body ?? string.Empty)
            };
        }

        private sealed class BrowserHttpResponse
        {
            public bool IsNetworkError { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public string? Body { get; set; }
        }
    }
}
